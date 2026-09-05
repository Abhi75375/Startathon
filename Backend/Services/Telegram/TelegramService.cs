using System.Net.Http.Json;

namespace Backend.Services.Telegram;

public class TelegramService : ITelegramService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TelegramService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendMessageAsync(
        string chatId,
        string message)
    {
        var botToken =
            _configuration["Telegram:BotToken"];

        if (string.IsNullOrWhiteSpace(botToken))
        {
            throw new InvalidOperationException(
                "Telegram BotToken is missing.");
        }

        if (string.IsNullOrWhiteSpace(chatId))
        {
            throw new ArgumentException(
                "Telegram chat ID is required.",
                nameof(chatId));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Telegram message is required.",
                nameof(message));
        }

        var url =
            $"https://api.telegram.org/bot{botToken}/sendMessage";

        var payload = new
        {
            chat_id = chatId,
            text = message
        };

        var response =
            await _httpClient.PostAsJsonAsync(
                url,
                payload);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Telegram API failed. Status: {StatusCode}, Response: {Response}",
                response.StatusCode,
                responseBody);

            throw new HttpRequestException(
                $"Telegram API returned " +
                $"{response.StatusCode}: {responseBody}");
        }
    }
}