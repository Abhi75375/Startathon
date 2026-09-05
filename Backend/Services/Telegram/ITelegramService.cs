namespace Backend.Services.Telegram;

public interface ITelegramService
{
    Task SendMessageAsync(
        string chatId,
        string message);

    Task StartReceivingAsync(
        CancellationToken cancellationToken);
}