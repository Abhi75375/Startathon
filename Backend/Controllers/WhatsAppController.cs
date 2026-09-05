using Backend.Services.WhatsApp;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _whatsAppService;

    public WhatsAppController(
        IWhatsAppService whatsAppService)
    {
        _whatsAppService = whatsAppService;
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test(
        [FromBody] SendWhatsAppTestRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest(
                "Phone number is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(
                "Message is required.");
        }

        try
        {
            var result =
                await _whatsAppService.SendTextMessageAsync(
                    request.PhoneNumber,
                    request.Message);

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    error = ex.Message
                });
        }
    }

    [HttpPost("template-test")]
    public async Task<IActionResult> TemplateTest(
        [FromBody] SendWhatsAppTemplateTestRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest(
                "Phone number is required.");
        }

        try
        {
            var result =
                await _whatsAppService.SendTemplateMessageAsync(
                    request.PhoneNumber,
                    request.TemplateName,
                    request.LanguageCode,
                    request.Parameters ?? []);

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    error = ex.Message
                });
        }
    }
}

public record SendWhatsAppTestRequest(
    string PhoneNumber,
    string Message);

public record SendWhatsAppTemplateTestRequest(
    string PhoneNumber,
    string TemplateName,
    string LanguageCode,
    string[]? Parameters);