using Backend.Contracts;
using Backend.Data;
using Backend.Models;

namespace Backend.Services;

public class ProcurementRequestService
{
    private readonly ProcurementDbContext _db;
    private readonly IProcurementApprovalGateway _approvalGateway;

    public ProcurementRequestService(ProcurementDbContext db, IProcurementApprovalGateway approvalGateway)
    {
        _db = db;
        _approvalGateway = approvalGateway;
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
            Quantity = request.ShortageQuantity,
            SupplierId = request.SelectedSupplierId,
            SupplierName = request.SelectedSupplierName!,
            UnitPrice = request.SelectedSupplierPrice.Value,
            TotalCost = request.ShortageQuantity * request.SelectedSupplierPrice.Value,
            EstimatedDeliveryDate = request.EstimatedDeliveryDate.Value,
            SupplierTelegramChatId = request.SelectedSupplierTelegramChatId!,
        };

        _db.ProcurementRequests.Add(procurementRequest);
        request.Status = MaterialRequestStatus.ProcurementRequested;

        await _db.SaveChangesAsync();

        // NEW - immediately notify the middleware so a manager can review it
        await _approvalGateway.SubmitForApprovalAsync(new ProcurementApprovalPayload(
            procurementRequest.Id,
            procurementRequest.MaterialCode,
            procurementRequest.Quantity,
            procurementRequest.SupplierName,
            procurementRequest.UnitPrice,
            procurementRequest.TotalCost,
            procurementRequest.EstimatedDeliveryDate
        ));

        return procurementRequest;
    }

    // NEW - the middleware calls this back with the manager's decision
    public async Task<ProcurementRequest> RecordDecisionAsync(Guid procurementRequestId, string decidedBy, bool approved, string? rejectionReason)
    {
        var procurementRequest = await _db.ProcurementRequests.FindAsync(procurementRequestId)
            ?? throw new InvalidOperationException("Procurement request not found");

        var materialRequest = await _db.MaterialRequests.FindAsync(procurementRequest.MaterialRequestId)
            ?? throw new InvalidOperationException("Linked material request not found");

        procurementRequest.Status = approved ? ProcurementRequestStatus.Approved : ProcurementRequestStatus.Rejected;
        procurementRequest.DecidedBy = decidedBy;
        procurementRequest.DecidedAt = DateTime.UtcNow;
        procurementRequest.RejectionReason = approved ? null : rejectionReason;

        materialRequest.Status = approved
            ? MaterialRequestStatus.ProcurementApproved
            : MaterialRequestStatus.ProcurementRejected;

        await _db.SaveChangesAsync();

        return procurementRequest;
    }
}