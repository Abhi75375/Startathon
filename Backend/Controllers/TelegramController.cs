using Backend.Services.Telegram;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramService _telegramService;

    public TelegramController(
        ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test(
        [FromBody] TelegramTestRequest request)
    {
        await _telegramService.SendMessageAsync(
            request.ChatId,
            request.Message);

        return Ok(new
        {
            message = "Telegram message sent successfully"
        });
    }
}

public record TelegramTestRequest(
    string ChatId,
    string Message);