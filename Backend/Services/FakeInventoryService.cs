using Backend.Contracts;

namespace Backend.Services;

public class FakeInventoryService : IInventoryService
{
    public Task<StockInfo> GetStockAsync(string materialCode)
        => Task.FromResult(new StockInfo(materialCode, CurrentStock: 50, ReservedStock: 10, IncomingStock: 0));

    public Task IncreaseStockAsync(string materialCode, decimal quantity)
    {
        Console.WriteLine($"[FAKE] Increased stock for {materialCode} by {quantity}");
        return Task.CompletedTask;
    }
}