// Backend/Services/ErpInventoryService.cs

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
        var response = await _http.GetAsync(
            $"warehouse/{Uri.EscapeDataString(materialCode)}");

        response.EnsureSuccessStatusCode();

        var item =
            await response.Content.ReadFromJsonAsync<ErpWarehouseItem>();

        if (item is null)
        {
            throw new InvalidOperationException(
                $"No warehouse data returned for material {materialCode}.");
        }

        return new StockInfo(
            MaterialCode: item.Sku,
            CurrentStock: Convert.ToDecimal(item.Quantity),
            ReservedStock: 0m,
            IncomingStock: 0m);
    }

    public async Task IncreaseStockAsync(
        string materialCode,
        decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero.");
        }

        var payload = new
        {
            operation = "add",
            amount = quantity
        };

        var response = await _http.PatchAsJsonAsync(
            $"warehouse/{Uri.EscapeDataString(materialCode)}",
            payload);

        response.EnsureSuccessStatusCode();
    }

    private sealed record ErpWarehouseItem(
        string Sku,
        string Name,
        string Category,
        string Unit,
        double Quantity,
        double MinStock,
        double UnitCost,
        string Supplier,
        string Location,
        string Notes,
        double TotalValue,
        bool LowStock);
}