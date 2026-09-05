using System.Net.Http.Json;
using Backend.Contracts;

namespace Backend.Services;

public class BudgetService : IBudgetService
{
    private readonly HttpClient _http;

    public BudgetService(HttpClient http)
    {
        _http = http;
    }

    public async Task<BudgetInfo> GetMaxBudgetAsync(string materialCode)
    {
        var response = await _http.GetAsync($"materials/{materialCode}/budget");
        response.EnsureSuccessStatusCode();

        var budget = await response.Content.ReadFromJsonAsync<BudgetInfo>();
        return budget ?? throw new InvalidOperationException($"No budget data for {materialCode}");
    }
}