namespace Backend.Services.WhatsApp;

public interface IWhatsAppService
{
    Task<string> SendTextMessageAsync(
        string phoneNumber,
        string message);

    Task<string> SendTemplateMessageAsync(
        string phoneNumber,
        string templateName,
        string languageCode,
        params string[] parameters);
}