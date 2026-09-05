using Backend.Contracts;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class InventoryCheckService
{
    private readonly ProcurementDbContext _db;
    private readonly IInventoryService _inventoryService;

    public InventoryCheckService(ProcurementDbContext db, IInventoryService inventoryService)
    {
        _db = db;
        _inventoryService = inventoryService;
    }

    public async Task<InventoryCheckResult> CheckAsync(Guid materialRequestId)
    {
        var request = await _db.MaterialRequests.FindAsync(materialRequestId)
            ?? throw new InvalidOperationException("Request not found");

        var stock = await _inventoryService.GetStockAsync(request.MaterialCode);

        // Available = current stock, minus what's already reserved for other requests,
        // plus what's already incoming from a prior PO
        decimal available = stock.CurrentStock - stock.ReservedStock + stock.IncomingStock;

        bool enough = available >= request.QuantityRequested;
        decimal shortage = enough ? 0 : request.QuantityRequested - available;

        request.Status = enough
            ? MaterialRequestStatus.Fulfilled
            : MaterialRequestStatus.ShortageIdentified;

        await _db.SaveChangesAsync();

        return new InventoryCheckResult(request.Id, enough, shortage, stock);
    }
}

public record InventoryCheckResult(Guid MaterialRequestId, bool SufficientStock, decimal Shortage, StockInfo Stock);