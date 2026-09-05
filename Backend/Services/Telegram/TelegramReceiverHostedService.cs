namespace Backend.Services.Telegram;

public class TelegramReceiverHostedService
    : BackgroundService
{
    private readonly ITelegramService _telegramService;
    private readonly ILogger<TelegramReceiverHostedService> _logger;

    public TelegramReceiverHostedService(
        ITelegramService telegramService,
        ILogger<TelegramReceiverHostedService> logger)
    {
        _telegramService = telegramService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting Telegram receiver.");

        await _telegramService.StartReceivingAsync(
            stoppingToken);
    }
}