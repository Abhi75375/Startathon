using System.Net.Http.Json;
using Backend.Contracts;

namespace Backend.Services;

public class ProcurementApprovalGateway : IProcurementApprovalGateway
{
    private readonly HttpClient _http;

    public ProcurementApprovalGateway(HttpClient http)
    {
        _http = http;
    }

    public async Task SubmitForApprovalAsync(ProcurementApprovalPayload payload)
    {
        var response = await _http.PostAsJsonAsync("procurement-requests", payload);
        response.EnsureSuccessStatusCode();
    }
}