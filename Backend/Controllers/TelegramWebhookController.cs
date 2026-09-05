using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/webhooks/telegram")]
public class TelegramWebhookController : ControllerBase
{
    private readonly ILogger<TelegramWebhookController> _logger;

    public TelegramWebhookController(
        ILogger<TelegramWebhookController> logger)
    {
        _logger = logger;
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
            using var json =
                JsonDocument.Parse(body);

            if (json.RootElement.TryGetProperty(
                    "message",
                    out var message))
            {
                var chatId =
                    message
                        .GetProperty("chat")
                        .GetProperty("id")
                        .GetInt64();

                var text =
                    message.TryGetProperty(
                        "text",
                        out var textProperty)
                        ? textProperty.GetString()
                        : null;

                var firstName =
                    message.TryGetProperty(
                        "from",
                        out var from) &&
                    from.TryGetProperty(
                        "first_name",
                        out var firstNameProperty)
                        ? firstNameProperty.GetString()
                        : "Unknown";

                _logger.LogInformation(
                    """
                    TELEGRAM WEBHOOK MESSAGE

                    Chat ID: {ChatId}
                    Sender: {Sender}
                    Text: {Text}
                    """,
                    chatId,
                    firstName,
                    text);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Invalid Telegram webhook payload.");
        }

        return Ok();
    }
}