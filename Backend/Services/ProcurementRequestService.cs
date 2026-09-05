using Backend.Data;
using Backend.Models;

namespace Backend.Services;

public class ProcurementRequestService
{
    private readonly ProcurementDbContext _db;

    public ProcurementRequestService(ProcurementDbContext db)
    {
        _db = db;
    }

    public async Task<ProcurementRequest> GenerateAsync(Guid materialRequestId)
    {
        var request = await _db.MaterialRequests.FindAsync(materialRequestId)
            ?? throw new InvalidOperationException("Material request not found");

        if (request.Status != MaterialRequestStatus.SupplierSelected)
            throw new InvalidOperationException(
                $"A supplier must be selected before generating a procurement request. Current status: {request.Status}");

        if (request.SelectedSupplierId is null || request.SelectedSupplierPrice is null || request.EstimatedDeliveryDate is null)
            throw new InvalidOperationException("Request is missing supplier selection details");

        var procurementRequest = new ProcurementRequest
        {
            MaterialRequestId = request.Id,
            MaterialCode = request.MaterialCode,
            Quantity = request.ShortageQuantity, // only the shortfall gets procured, not the full original quantity
            SupplierId = request.SelectedSupplierId,
            SupplierName = request.SelectedSupplierName!,
            UnitPrice = request.SelectedSupplierPrice.Value,
            TotalCost = request.ShortageQuantity * request.SelectedSupplierPrice.Value,
            EstimatedDeliveryDate = request.EstimatedDeliveryDate.Value
        };

        _db.ProcurementRequests.Add(procurementRequest);

        request.Status = MaterialRequestStatus.ProcurementRequested;

        await _db.SaveChangesAsync();

        return procurementRequest;
    }
}