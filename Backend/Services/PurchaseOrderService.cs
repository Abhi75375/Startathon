using Backend.Contracts;
using Backend.Data;
using Backend.Models;

namespace Backend.Services;

public class PurchaseOrderService
{
    private readonly ProcurementDbContext _db;
    private readonly IPoApprovalGateway _poApprovalGateway;

    public PurchaseOrderService(ProcurementDbContext db, IPoApprovalGateway poApprovalGateway)
    {
        _db = db;
        _poApprovalGateway = poApprovalGateway;
    }

    // Step 7: PO Generation - triggered once a ProcurementRequest is approved
    public async Task<PurchaseOrder> GenerateAsync(Guid procurementRequestId)
    {
        var procurementRequest = await _db.ProcurementRequests.FindAsync(procurementRequestId)
            ?? throw new InvalidOperationException("Procurement request not found");

        if (procurementRequest.Status != ProcurementRequestStatus.Approved)
            throw new InvalidOperationException(
                $"Procurement request must be Approved before generating a PO. Current status: {procurementRequest.Status}");

        var materialRequest = await _db.MaterialRequests.FindAsync(procurementRequest.MaterialRequestId)
            ?? throw new InvalidOperationException("Linked material request not found");

        var po = new PurchaseOrder
        {
            ProcurementRequestId = procurementRequest.Id,
            MaterialRequestId = materialRequest.Id,
            PoNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            MaterialCode = procurementRequest.MaterialCode,
            Quantity = procurementRequest.Quantity,
            SupplierId = procurementRequest.SupplierId,
            SupplierName = procurementRequest.SupplierName,
            UnitPrice = procurementRequest.UnitPrice,
            TotalCost = procurementRequest.TotalCost,
            EstimatedDeliveryDate = procurementRequest.EstimatedDeliveryDate,
            SupplierTelegramChatId = procurementRequest.SupplierTelegramChatId
        };

        _db.PurchaseOrders.Add(po);
        materialRequest.Status = MaterialRequestStatus.PoPendingApproval;

        await _db.SaveChangesAsync();

        // Notify the middleware so a manager/supervisor can review the PO
        await _poApprovalGateway.SubmitForApprovalAsync(new PoApprovalPayload(
            po.Id, po.PoNumber, po.MaterialCode, po.Quantity, po.SupplierName, po.UnitPrice, po.TotalCost, po.EstimatedDeliveryDate
        ));

        return po;
    }

    // Step 8: PO Approval - the middleware calls this back with the decision
    public async Task<PurchaseOrder> RecordDecisionAsync(Guid purchaseOrderId, string decidedBy, bool approved, string? rejectionReason)
    {
        var po = await _db.PurchaseOrders.FindAsync(purchaseOrderId)
            ?? throw new InvalidOperationException("Purchase order not found");

        var materialRequest = await _db.MaterialRequests.FindAsync(po.MaterialRequestId)
            ?? throw new InvalidOperationException("Linked material request not found");

        po.Status = approved ? PurchaseOrderStatus.Approved : PurchaseOrderStatus.Rejected;
        po.DecidedBy = decidedBy;
        po.DecidedAt = DateTime.UtcNow;
        po.RejectionReason = approved ? null : rejectionReason;

        materialRequest.Status = approved
            ? MaterialRequestStatus.PoApproved
            : MaterialRequestStatus.PoRejected;

        await _db.SaveChangesAsync();

        return po;
    }
}