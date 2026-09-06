// Backend/Contracts/IInventoryService.cs

namespace Backend.Contracts;

public interface IInventoryService
{
    Task<StockInfo> GetStockAsync(string materialCode);

    Task IncreaseStockAsync(
        string materialCode,
        decimal quantity);
}

public record StockInfo(
    string MaterialCode,
    decimal CurrentStock,
    decimal ReservedStock,
    decimal IncomingStock);