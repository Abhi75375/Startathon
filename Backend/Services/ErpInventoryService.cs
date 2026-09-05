using System.Net.Http.Json;
using Backend.Contracts;

namespace Backend.Services;

public class ErpInventoryService : IInventoryService
{
    private readonly HttpClient _http;

    public ErpInventoryService(HttpClient http)
    {
        _http = http;
    }

    public async Task<StockInfo> GetStockAsync(string materialCode)
    {
        var response = await _http.GetAsync($"materials/{materialCode}/stock");
        response.EnsureSuccessStatusCode();

        var stock = await response.Content.ReadFromJsonAsync<StockInfo>();
        return stock ?? throw new InvalidOperationException($"No stock data for {materialCode}");
    }
}