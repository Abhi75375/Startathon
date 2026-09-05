using System.Net.Http.Json;
using Backend.Contracts;

namespace Backend.Services;

public class SupplierService : ISupplierService
{
    private readonly HttpClient _http;

    public SupplierService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<SupplierOffer>> GetSuppliersAsync(string materialCode)
    {
        var response = await _http.GetAsync($"materials/{materialCode}/suppliers");
        response.EnsureSuccessStatusCode();

        var offers = await response.Content.ReadFromJsonAsync<List<SupplierOffer>>();
        return offers ?? new List<SupplierOffer>();
    }
}