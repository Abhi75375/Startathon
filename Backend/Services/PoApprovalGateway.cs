using System.Net.Http.Json;
using Backend.Contracts;

namespace Backend.Services;

public class PoApprovalGateway : IPoApprovalGateway
{
    private readonly HttpClient _http;

    public PoApprovalGateway(HttpClient http)
    {
        _http = http;
    }

    public async Task SubmitForApprovalAsync(PoApprovalPayload payload)
    {
        var response = await _http.PostAsJsonAsync("purchase-orders", payload);
        response.EnsureSuccessStatusCode();
    }
}