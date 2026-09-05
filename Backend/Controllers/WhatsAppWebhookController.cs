using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/webhooks/whatsapp")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppWebhookController> _logger;

    public WhatsAppWebhookController(
        IConfiguration configuration,
        ILogger<WhatsAppWebhookController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // Meta uses this GET request to verify your webhook.
    [HttpGet]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        var expectedToken =
            _configuration["WhatsApp:VerifyToken"];

        if (mode == "subscribe" &&
            !string.IsNullOrWhiteSpace(verifyToken) &&
            verifyToken == expectedToken)
        {
            _logger.LogInformation(
                "WhatsApp webhook verified successfully.");

            return Ok(challenge);
        }

        _logger.LogWarning(
            "WhatsApp webhook verification failed.");

        return Unauthorized();
    }

    // Meta sends incoming messages and other WhatsApp events here.
    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook()
    {
        using var reader =
            new StreamReader(Request.Body);

        var body = await reader.ReadToEndAsync();

        _logger.LogInformation(
            "WhatsApp webhook received:\n{Body}",
            body);

        try
        {
            using var json =
                JsonDocument.Parse(body);

            ProcessWebhook(json);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Invalid JSON received from WhatsApp.");

            return BadRequest();
        }

        // Important: respond quickly with 200.
        return Ok();
    }

    private void ProcessWebhook(JsonDocument json)
    {
        if (!json.RootElement.TryGetProperty(
                "entry",
                out var entries))
        {
            return;
        }

        foreach (var entry in entries.EnumerateArray())
        {
            if (!entry.TryGetProperty(
                    "changes",
                    out var changes))
            {
                continue;
            }

            foreach (var change in changes.EnumerateArray())
            {
                if (!change.TryGetProperty(
                        "value",
                        out var value))
                {
                    continue;
                }

                // Incoming WhatsApp messages
                if (value.TryGetProperty(
                        "messages",
                        out var messages))
                {
                    foreach (var message in messages.EnumerateArray())
                    {
                        ProcessIncomingMessage(message);
                    }
                }

                // Delivery/read/failed status notifications
                if (value.TryGetProperty(
                        "statuses",
                        out var statuses))
                {
                    foreach (var status in statuses.EnumerateArray())
                    {
                        ProcessMessageStatus(status);
                    }
                }
            }
        }
    }

    private void ProcessIncomingMessage(
        JsonElement message)
    {
        var messageId =
            message.TryGetProperty("id", out var id)
                ? id.GetString()
                : null;

        var sender =
            message.TryGetProperty("from", out var from)
                ? from.GetString()
                : null;

        var messageType =
            message.TryGetProperty("type", out var type)
                ? type.GetString()
                : null;

        string? text = null;

        if (messageType == "text" &&
            message.TryGetProperty(
                "text",
                out var textObject) &&
            textObject.TryGetProperty(
                "body",
                out var body))
        {
            text = body.GetString();
        }

        _logger.LogInformation(
            """
            INCOMING WHATSAPP MESSAGE
            Message ID: {MessageId}
            Sender: {Sender}
            Type: {Type}
            Text: {Text}
            """,
            messageId,
            sender,
            messageType,
            text);
    }

    private void ProcessMessageStatus(
        JsonElement status)
    {
        var messageId =
            status.TryGetProperty("id", out var id)
                ? id.GetString()
                : null;

        var statusValue =
            status.TryGetProperty("status", out var statusProperty)
                ? statusProperty.GetString()
                : null;

        _logger.LogInformation(
            "WhatsApp message status: {MessageId} -> {Status}",
            messageId,
            statusValue);
    }
}