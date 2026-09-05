using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Backend.Services.WhatsApp;

public class MetaWhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public MetaWhatsAppService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> SendTextMessageAsync(
        string phoneNumber,
        string message)
    {
        var phoneNumberId =
            _configuration["WhatsApp:PhoneNumberId"];

        var accessToken =
            _configuration["WhatsApp:AccessToken"];

        if (string.IsNullOrWhiteSpace(phoneNumberId))
        {
            throw new InvalidOperationException(
                "WhatsApp PhoneNumberId is missing.");
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "WhatsApp AccessToken is missing.");
        }

        const string graphApiVersion = "v25.0";

        var url =
            $"https://graph.facebook.com/{graphApiVersion}/{phoneNumberId}/messages";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = phoneNumber,
            type = "text",
            text = new
            {
                preview_url = false,
                body = message
            }
        };

        request.Content = JsonContent.Create(payload);

        var response =
            await _httpClient.SendAsync(request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Meta WhatsApp API error: " +
                $"{response.StatusCode}\n" +
                responseBody);
        }

        return responseBody;
    }

    public async Task<string> SendTemplateMessageAsync(
        string phoneNumber,
        string templateName,
        string languageCode,
        params string[] parameters)
    {
        var phoneNumberId =
            _configuration["WhatsApp:PhoneNumberId"];

        var accessToken =
            _configuration["WhatsApp:AccessToken"];

        if (string.IsNullOrWhiteSpace(phoneNumberId))
        {
            throw new InvalidOperationException(
                "WhatsApp PhoneNumberId is missing.");
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "WhatsApp AccessToken is missing.");
        }

        const string graphApiVersion = "v25.0";

        var url =
            $"https://graph.facebook.com/{graphApiVersion}/{phoneNumberId}/messages";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        object[] components;

        if (parameters.Length > 0)
        {
            components =
            [
                new
                {
                    type = "body",
                    parameters = parameters
                        .Select(value => new
                        {
                            type = "text",
                            text = value
                        })
                        .ToArray()
                }
            ];
        }
        else
        {
            components = [];
        }

        var payload = new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = phoneNumber,
            type = "template",
            template = new
            {
                name = templateName,
                language = new
                {
                    code = languageCode
                },
                components
            }
        };

        request.Content = JsonContent.Create(payload);

        var response =
            await _httpClient.SendAsync(request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Meta WhatsApp API error: " +
                $"{response.StatusCode}\n" +
                responseBody);
        }

        return responseBody;
    }
}