namespace Backend.Contracts;

public interface IInventoryService
{
    Task<StockInfo> GetStockAsync(string materialCode);
}

public record StockInfo(
    string MaterialCode,
    decimal CurrentStock,
    decimal ReservedStock,
    decimal IncomingStock
);