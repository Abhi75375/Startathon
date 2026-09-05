using System.Net.Http.Json;
using Backend.Contracts;

namespace Backend.Services;

public class SupervisorReviewGateway : ISupervisorReviewGateway
{
    private readonly HttpClient _http;

    public SupervisorReviewGateway(HttpClient http)
    {
        _http = http;
    }

    public async Task SubmitForReviewAsync(Guid reviewId, Guid projectId, List<ReviewItemPayload> items)
    {
        var payload = new { reviewId, projectId, items };
        var response = await _http.PostAsJsonAsync("material-estimation-reviews", payload);
        response.EnsureSuccessStatusCode();
    }
}