using System.Net.Http.Json;
using Backend.Contracts;

namespace Backend.Services;

public class OrderNotificationGateway : IOrderNotificationGateway
{
    private readonly HttpClient _http;

    public OrderNotificationGateway(HttpClient http) => _http = http;

    public async Task SendOrderAsync(OrderNotificationPayload payload)
    {
        var response = await _http.PostAsJsonAsync("supplier-orders", payload);
        response.EnsureSuccessStatusCode();
    }
}