using System.Text.Json;
using System.Text.RegularExpressions;
using Backend.Data;
using Backend.Models;
using Backend.Services;
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

    public TelegramWebhookController(
        ILogger<TelegramWebhookController> logger,
        ProcurementDbContext db,
        DeliveryTrackingService deliveryTrackingService)
    {
        _logger = logger;
        _db = db;
        _deliveryTrackingService = deliveryTrackingService;
    }

    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        _logger.LogInformation("Telegram webhook received:\n{Body}", body);

        try
        {
            using var json = JsonDocument.Parse(body);

            if (json.RootElement.TryGetProperty("message", out var message))
            {
                var chatId = message.GetProperty("chat").GetProperty("id").GetInt64().ToString();
                var text = message.TryGetProperty("text", out var textProperty) ? textProperty.GetString() : null;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    await HandleVendorReplyAsync(chatId, text);
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid Telegram webhook payload.");
        }

        return Ok();
    }

    // NEW - the actual connection point into your procurement workflow
    private async Task HandleVendorReplyAsync(string chatId, string text)
    {
        var po = await _db.PurchaseOrders
            .Where(p => p.SupplierTelegramChatId == chatId && p.VendorConfirmationStatus == VendorConfirmationStatus.Pending)
            .OrderByDescending(p => p.SentForConfirmationAt)
            .FirstOrDefaultAsync();

        if (po is null)
        {
            _logger.LogInformation("Received Telegram message from {ChatId} but no PO is awaiting their confirmation.", chatId);
            return;
        }

        var lower = text.Trim().ToLowerInvariant();

        // Basic keyword heuristic for now - replace with something smarter later if replies get varied/ambiguous
        bool? sufficientStock = lower.Contains("no") ? false : lower.Contains("yes") ? true : null;

        if (sufficientStock is null)
        {
            _logger.LogWarning("Could not interpret vendor reply from {ChatId}: \"{Text}\" - ignoring.", chatId, text);
            return;
        }

        var quantityMatch = Regex.Match(text, @"\d+(\.\d+)?");
        decimal? availableQuantity = quantityMatch.Success ? decimal.Parse(quantityMatch.Value) : null;

        await _deliveryTrackingService.RecordVendorResponseAsync(po.Id, sufficientStock.Value, availableQuantity, text);
    }
}