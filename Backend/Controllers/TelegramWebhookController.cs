using System.Text.Json;
using Backend.Data;
using Backend.Models;
using Backend.Services;
using Backend.Services.Telegram;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/webhooks/telegram")]
public class TelegramWebhookController : ControllerBase
{
    private readonly ILogger<TelegramWebhookController> _logger;
    private readonly ProcurementDbContext _db;
    private readonly DeliveryTrackingService _deliveryTrackingService;
    private readonly IVendorReplyParser _vendorReplyParser;

    public TelegramWebhookController(
        ILogger<TelegramWebhookController> logger,
        ProcurementDbContext db,
        DeliveryTrackingService deliveryTrackingService,
        IVendorReplyParser vendorReplyParser)
    {
        _logger = logger;
        _db = db;
        _deliveryTrackingService = deliveryTrackingService;
        _vendorReplyParser = vendorReplyParser;
    }

    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        _logger.LogInformation(
            "Telegram webhook received:\n{Body}",
            body);

        try
        {
            using var json = JsonDocument.Parse(body);

            if (!json.RootElement.TryGetProperty("message", out var message))
            {
                _logger.LogInformation(
                    "Telegram update does not contain a message.");

                return Ok();
            }

            var chatId = message
                .GetProperty("chat")
                .GetProperty("id")
                .GetInt64()
                .ToString();

            var text =
                message.TryGetProperty("text", out var textProperty)
                    ? textProperty.GetString()
                    : null;

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogInformation(
                    "Telegram message from {ChatId} contains no text.",
                    chatId);

                return Ok();
            }

            await HandleVendorReplyAsync(chatId, text);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Invalid Telegram webhook payload.");

            return BadRequest(new
            {
                error = "Invalid Telegram webhook payload."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing Telegram webhook.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    error = "Failed to process Telegram webhook."
                });
        }

        return Ok();
    }

    private async Task HandleVendorReplyAsync(
        string chatId,
        string text)
    {
        _logger.LogInformation(
            "Processing vendor reply from ChatId {ChatId}: {Text}",
            chatId,
            text);

        // --------------------------------------------------------
        // Find the most recent PO waiting for this vendor's reply.
        // --------------------------------------------------------

        var po = await _db.PurchaseOrders
            .Where(p =>
                p.SupplierTelegramChatId == chatId &&
                p.VendorConfirmationStatus ==
                    VendorConfirmationStatus.Pending)
            .OrderByDescending(p => p.SentForConfirmationAt)
            .FirstOrDefaultAsync();

        if (po is null)
        {
            _logger.LogInformation(
                "Received Telegram message from {ChatId}, " +
                "but no purchase order is awaiting confirmation.",
                chatId);

            return;
        }

        _logger.LogInformation(
            "Matched Telegram reply to PurchaseOrder {PurchaseOrderId} " +
            "(PO {PoNumber}).",
            po.Id,
            po.PoNumber);

        // --------------------------------------------------------
        // Parse:
        //
        // YES
        // YES 60
        // NO
        // --------------------------------------------------------

        var reply = _vendorReplyParser.Parse(text);

        if (reply is null)
        {
            _logger.LogWarning(
                "Could not understand vendor response from {ChatId}: {Text}",
                chatId,
                text);

            return;
        }

        _logger.LogInformation(
            "Vendor response parsed. " +
            "CanSupply={CanSupply}, AvailableQuantity={AvailableQuantity}",
            reply.CanSupply,
            reply.AvailableQuantity);

        // --------------------------------------------------------
        // Pass the decision into the procurement/delivery logic.
        //
        // DeliveryTrackingService decides:
        //
        // YES + enough quantity
        //      -> Confirm order
        //
        // NO / insufficient quantity
        //      -> Reject supplier
        //      -> Find next supplier
        //      -> Resend PO to next supplier
        // --------------------------------------------------------

        await _deliveryTrackingService.RecordVendorResponseAsync(
            po.Id,
            reply.CanSupply,
            reply.AvailableQuantity,
            text);
    }
}