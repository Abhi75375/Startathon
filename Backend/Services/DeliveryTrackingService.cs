using Backend.Contracts;
using Backend.Data;
using Backend.Models;

namespace Backend.Services;

public class DeliveryTrackingService
{
    private readonly ProcurementDbContext _db;
    private readonly IOrderNotificationGateway _orderNotificationGateway;
    private readonly IInventoryService _inventoryService;

    public DeliveryTrackingService(
        ProcurementDbContext db,
        IOrderNotificationGateway orderNotificationGateway,
        IInventoryService inventoryService)
    {
        _db = db;
        _orderNotificationGateway = orderNotificationGateway;
        _inventoryService = inventoryService;
    }

    // Step 9: Send Order
    public async Task<PurchaseOrder> SendOrderAsync(Guid purchaseOrderId)
    {
        var po = await _db.PurchaseOrders.FindAsync(purchaseOrderId)
            ?? throw new InvalidOperationException("Purchase order not found");

        if (po.Status != PurchaseOrderStatus.Approved)
            throw new InvalidOperationException($"PO must be Approved before sending. Current status: {po.Status}");

        var materialRequest = await _db.MaterialRequests.FindAsync(po.MaterialRequestId)
            ?? throw new InvalidOperationException("Linked material request not found");

        await _orderNotificationGateway.SendOrderAsync(new OrderNotificationPayload(
            po.Id, po.PoNumber, po.SupplierId, po.SupplierName, po.MaterialCode, po.Quantity, po.TotalCost, po.EstimatedDeliveryDate
        ));

        po.DeliveryStatus = DeliveryStatus.Ordered;
        po.OrderedAt = DateTime.UtcNow;
        materialRequest.Status = MaterialRequestStatus.Ordered;

        await _db.SaveChangesAsync();
        return po;
    }

    // Step 10: Delivery Tracking - status progression, with optional quantity for partial/full deliveries
    public async Task<PurchaseOrder> UpdateDeliveryStatusAsync(Guid purchaseOrderId, DeliveryStatus newStatus, decimal? deliveredQuantityThisEvent)
    {
        var po = await _db.PurchaseOrders.FindAsync(purchaseOrderId)
            ?? throw new InvalidOperationException("Purchase order not found");

        po.DeliveryStatus = newStatus;

        // Step 11: Inventory Update - every delivery event (partial or full) increases inventory immediately
        if ((newStatus == DeliveryStatus.PartiallyDelivered || newStatus == DeliveryStatus.Delivered)
            && deliveredQuantityThisEvent is > 0)
        {
            po.DeliveredQuantity += deliveredQuantityThisEvent.Value;
            await _inventoryService.IncreaseStockAsync(po.MaterialCode, deliveredQuantityThisEvent.Value);
        }

        if (newStatus == DeliveryStatus.Delivered)
        {
            po.ActualDeliveryDate = DateTime.UtcNow;
            await RecordSupplierPerformanceAsync(po);
        }

        await _db.SaveChangesAsync();
        return po;
    }

    // Step 12: Supplier Performance - runs automatically once fully Delivered
    private async Task RecordSupplierPerformanceAsync(PurchaseOrder po)
    {
        var actualDate = po.ActualDeliveryDate ?? DateTime.UtcNow;
        bool onTime = actualDate <= po.EstimatedDeliveryDate;
        int daysLate = onTime ? 0 : (int)(actualDate - po.EstimatedDeliveryDate).TotalDays;

        var record = new SupplierPerformanceRecord
        {
            PurchaseOrderId = po.Id,
            SupplierId = po.SupplierId,
            SupplierName = po.SupplierName,
            MaterialCode = po.MaterialCode,
            EstimatedDeliveryDate = po.EstimatedDeliveryDate,
            ActualDeliveryDate = actualDate,
            OnTime = onTime,
            DaysLate = daysLate
        };

        _db.SupplierPerformanceRecords.Add(record);
        // Note: this is recorded but not yet fed back into ISupplierService's ReliabilityScore -
        // that loop-back (using real historical performance to influence future Step 4 scoring)
        // is a natural next enhancement once there's enough real data, not required right now.
    }
}