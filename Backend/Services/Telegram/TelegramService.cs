using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;

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
        var botToken = GetBotToken();

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
                "Telegram sendMessage failed. " +
                "Status: {StatusCode}, Response: {Response}",
                response.StatusCode,
                responseBody);

            throw new HttpRequestException(
                $"Telegram API returned " +
                $"{response.StatusCode}: {responseBody}");
        }
    }

public async Task StartReceivingAsync(
    CancellationToken cancellationToken)
{
    var botToken = GetBotToken();

    long offset = 0;

    _logger.LogInformation(
        "Telegram message receiver started.");

    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            var baseUrl =
                $"https://api.telegram.org/bot{botToken}/getUpdates";

            var query = new Dictionary<string, string>
            {
                ["timeout"] = "30",
                ["offset"] = offset.ToString()
            };

            var url =
                QueryHelpers.AddQueryString(
                    baseUrl,
                    query);

            _logger.LogDebug(
                "Calling Telegram getUpdates with offset {Offset}",
                offset);

            using var response =
                await _httpClient.GetAsync(
                    url,
                    cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Telegram getUpdates failed. " +
                    "Status: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    responseBody);

                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    cancellationToken);

                continue;
            }

            var result =
                JsonSerializer.Deserialize<TelegramUpdateResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result?.Ok != true ||
                result.Result is null)
            {
                continue;
            }

            foreach (var update in result.Result)
            {
                _logger.LogInformation(
                    "Processing Telegram update {UpdateId}",
                    update.UpdateId);

                ProcessUpdate(update);

                // Move offset AFTER processing.
                offset = update.UpdateId + 1;
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            break;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while receiving Telegram updates.");

            await Task.Delay(
                TimeSpan.FromSeconds(2),
                cancellationToken);
        }
    }

    _logger.LogInformation(
        "Telegram message receiver stopped.");
}

    private void ProcessUpdate(
        TelegramUpdate update)
    {
        var message = update.Message;

        if (message is null)
        {
            return;
        }

        var chatId =
            message.Chat?.Id;

        var senderName =
            message.From?.FirstName;

        var text =
            message.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogInformation(
                "Received non-text Telegram message " +
                "from chat {ChatId}.",
                chatId);

            return;
        }

        _logger.LogInformation(
            """
            TELEGRAM MESSAGE RECEIVED

            Update ID: {UpdateId}
            Chat ID: {ChatId}
            Sender: {Sender}
            Text: {Text}
            """,
            update.UpdateId,
            chatId,
            senderName,
            text);

        // This is where we'll eventually pass the
        // supplier response into your procurement system.
    }

    private string GetBotToken()
    {
        var botToken =
            _configuration["Telegram:BotToken"];

        if (string.IsNullOrWhiteSpace(botToken))
        {
            throw new InvalidOperationException(
                "Telegram:BotToken is missing.");
        }

        return botToken;
    }
}


// ============================================================
// Telegram API response models
// ============================================================

public class TelegramUpdateResponse
{
    public bool Ok { get; set; }

    public List<TelegramUpdate>? Result { get; set; }
}


public class TelegramUpdate
{
    public long UpdateId { get; set; }
    public TelegramMessage? Message { get; set; }
}


public class TelegramMessage
{
    public long MessageId { get; set; }

    public TelegramUser? From { get; set; }

    public TelegramChat? Chat { get; set; }

    public string? Text { get; set; }
}


public class TelegramUser
{
    public long Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }
}


public class TelegramChat
{
    public long Id { get; set; }

    public string? Type { get; set; }

    public string? FirstName { get; set; }

    public string? Username { get; set; }
}