using Backend.Contracts;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class DeliveryTrackingService
{
    private readonly ProcurementDbContext _db;
    private readonly IOrderNotificationGateway _orderNotificationGateway;
    private readonly IInventoryService _inventoryService;
    private readonly SupplierSelectionService _supplierSelectionService;

    public DeliveryTrackingService(
        ProcurementDbContext db,
        IOrderNotificationGateway orderNotificationGateway,
        IInventoryService inventoryService,
        SupplierSelectionService supplierSelectionService)
    {
        _db = db;
        _orderNotificationGateway = orderNotificationGateway;
        _inventoryService = inventoryService;
        _supplierSelectionService = supplierSelectionService;
    }

    public async Task<PurchaseOrder> SendOrderAsync(
        Guid purchaseOrderId)
    {
        var po =
            await _db.PurchaseOrders.FindAsync(
                purchaseOrderId)
            ?? throw new InvalidOperationException(
                "Purchase order not found");

        if (po.Status != PurchaseOrderStatus.Approved)
        {
            throw new InvalidOperationException(
                $"PO must be Approved before sending. " +
                $"Current status: {po.Status}");
        }

        var materialRequest =
            await _db.MaterialRequests.FindAsync(
                po.MaterialRequestId)
            ?? throw new InvalidOperationException(
                "Linked material request not found");

        await _orderNotificationGateway.SendOrderAsync(
            new OrderNotificationPayload(
                po.Id,
                po.PoNumber,
                po.SupplierId,
                po.SupplierName,
                po.MaterialCode,
                po.Quantity,
                po.TotalCost,
                po.EstimatedDeliveryDate,
                po.SupplierTelegramChatId));

        po.VendorConfirmationStatus =
            VendorConfirmationStatus.Pending;

        po.SentForConfirmationAt =
            DateTime.UtcNow;

        materialRequest.Status =
            MaterialRequestStatus.AwaitingVendorConfirmation;

        await _db.SaveChangesAsync();

        return po;
    }

    public async Task<PurchaseOrder> RecordVendorResponseAsync(
        Guid purchaseOrderId,
        bool sufficientStock,
        decimal? availableQuantity,
        string? rawMessage = null)
    {
        var po =
            await _db.PurchaseOrders.FindAsync(
                purchaseOrderId)
            ?? throw new InvalidOperationException(
                "Purchase order not found");

        var materialRequest =
            await _db.MaterialRequests.FindAsync(
                po.MaterialRequestId)
            ?? throw new InvalidOperationException(
                "Linked material request not found");

        po.VendorRespondedAt =
            DateTime.UtcNow;

        decimal suppliedQuantity =
            sufficientStock
                ? availableQuantity ?? po.Quantity
                : 0;

        suppliedQuantity =
            Math.Min(
                suppliedQuantity,
                po.Quantity);

        po.VendorConfirmedQuantity =
            suppliedQuantity;

        decimal remainingQuantity =
            po.Quantity - suppliedQuantity;

        // --------------------------------------------------------
        // FULL FULFILLMENT
        // --------------------------------------------------------

        if (remainingQuantity <= 0)
        {
            po.VendorConfirmationStatus =
                VendorConfirmationStatus.Confirmed;

            po.DeliveryStatus =
                DeliveryStatus.Ordered;

            po.OrderedAt =
                DateTime.UtcNow;

            materialRequest.ShortageQuantity = 0;

            materialRequest.Status =
                MaterialRequestStatus.Ordered;

            await _db.SaveChangesAsync();

            return po;
        }

        // --------------------------------------------------------
        // PARTIAL / FAILED FULFILLMENT
        // --------------------------------------------------------

        po.VendorConfirmationStatus =
            VendorConfirmationStatus.Declined;

        materialRequest.ShortageQuantity =
            remainingQuantity;

        await _db.SaveChangesAsync();

        await EscalateToNextVendorAsync(
            po,
            remainingQuantity);

        return po;
    }

    private async Task EscalateToNextVendorAsync(
        PurchaseOrder failedPo,
        decimal remainingQuantity)
    {
        var materialRequest =
            await _db.MaterialRequests.FindAsync(
                failedPo.MaterialRequestId)
            ?? throw new InvalidOperationException(
                "Linked material request not found");

        var excludedIds =
            (materialRequest.ExcludedSupplierIds ?? "")
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .ToHashSet();

        excludedIds.Add(
            failedPo.SupplierId);

        materialRequest.ExcludedSupplierIds =
            string.Join(",", excludedIds);

        await _db.SaveChangesAsync();

        var (nextOffer, reason) =
            await _supplierSelectionService
                .FindNextBestOfferAsync(
                    materialRequest.Id);

        if (nextOffer is null)
        {
            materialRequest.Status =
                MaterialRequestStatus.NoSupplierAvailable;

            await _db.SaveChangesAsync();

            return;
        }

        // Reuse the same PO for escalation.
        failedPo.SupplierId =
            nextOffer.SupplierId;

        failedPo.SupplierName =
            nextOffer.SupplierName;

        failedPo.SupplierTelegramChatId =
            nextOffer.TelegramChatId;

        failedPo.Quantity =
            remainingQuantity;

        failedPo.UnitPrice =
            nextOffer.PricePerUnit;

        failedPo.TotalCost =
            remainingQuantity *
            nextOffer.PricePerUnit;

        failedPo.EstimatedDeliveryDate =
            nextOffer.DeliveryDate;

        failedPo.VendorConfirmationStatus =
            VendorConfirmationStatus.NotSent;

        failedPo.VendorRespondedAt = null;

        failedPo.VendorConfirmedQuantity = null;

        await _db.SaveChangesAsync();

        await SendOrderAsync(
            failedPo.Id);
    }

    public async Task<PurchaseOrder>
        UpdateDeliveryStatusAsync(
            Guid purchaseOrderId,
            DeliveryStatus newStatus,
            decimal? deliveredQuantityThisEvent)
    {
        var po =
            await _db.PurchaseOrders.FindAsync(
                purchaseOrderId)
            ?? throw new InvalidOperationException(
                "Purchase order not found");

        po.DeliveryStatus =
            newStatus;

        if ((newStatus ==
                 DeliveryStatus.PartiallyDelivered ||
             newStatus ==
                 DeliveryStatus.Delivered)
            && deliveredQuantityThisEvent is > 0)
        {
            po.DeliveredQuantity +=
                deliveredQuantityThisEvent.Value;

            await _inventoryService
                .IncreaseStockAsync(
                    po.MaterialCode,
                    deliveredQuantityThisEvent.Value);
        }

        if (newStatus ==
            DeliveryStatus.Delivered)
        {
            po.ActualDeliveryDate =
                DateTime.UtcNow;

            await RecordSupplierPerformanceAsync(
                po);
        }

        await _db.SaveChangesAsync();

        return po;
    }

    private async Task RecordSupplierPerformanceAsync(
        PurchaseOrder po)
    {
        var actualDate =
            po.ActualDeliveryDate
            ?? DateTime.UtcNow;

        bool onTime =
            actualDate <=
            po.EstimatedDeliveryDate;

        int daysLate =
            onTime
                ? 0
                : (int)(
                    actualDate -
                    po.EstimatedDeliveryDate)
                    .TotalDays;

        _db.SupplierPerformanceRecords.Add(
            new SupplierPerformanceRecord
            {
                PurchaseOrderId =
                    po.Id,

                SupplierId =
                    po.SupplierId,

                SupplierName =
                    po.SupplierName,

                MaterialCode =
                    po.MaterialCode,

                EstimatedDeliveryDate =
                    po.EstimatedDeliveryDate,

                ActualDeliveryDate =
                    actualDate,

                OnTime =
                    onTime,

                DaysLate =
                    daysLate
            });
    }
}