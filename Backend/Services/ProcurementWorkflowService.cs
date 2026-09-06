using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ProcurementWorkflowService : IProcurementWorkflowService
{
    private readonly ProcurementDbContext _db;
    private readonly InventoryCheckService _inventoryCheckService;
    private readonly VendorApprovalService _vendorApprovalService;
    private readonly ProcurementRequestService _procurementRequestService;
    private readonly PurchaseOrderService _purchaseOrderService;
    private readonly DeliveryTrackingService _deliveryTrackingService;
    private readonly ILogger<ProcurementWorkflowService> _logger;

    public ProcurementWorkflowService(
        ProcurementDbContext db,
        InventoryCheckService inventoryCheckService,
        VendorApprovalService vendorApprovalService,
        ProcurementRequestService procurementRequestService,
        PurchaseOrderService purchaseOrderService,
        DeliveryTrackingService deliveryTrackingService,
        ILogger<ProcurementWorkflowService> logger)
    {
        _db = db;
        _inventoryCheckService = inventoryCheckService;
        _vendorApprovalService = vendorApprovalService;
        _procurementRequestService = procurementRequestService;
        _purchaseOrderService = purchaseOrderService;
        _deliveryTrackingService = deliveryTrackingService;
        _logger = logger;
    }

    // ============================================================
    // START WORKFLOW
    //
    // MaterialRequest
    //      ↓
    // Inventory Check
    //      ↓
    // Shortage
    //      ↓
    // Vendor calculation
    //      ↓
    // External vendor approval
    //      ↓
    // WAIT
    // ============================================================

    public async Task StartFromMaterialRequestAsync(
        Guid materialRequestId)
    {
        var request = await _db.MaterialRequests
            .FirstOrDefaultAsync(x => x.Id == materialRequestId)
            ?? throw new InvalidOperationException(
                "Material request not found.");

        _logger.LogInformation(
            "Starting procurement workflow for MaterialRequest {MaterialRequestId}",
            materialRequestId);

        var inventoryResult =
            await _inventoryCheckService.CheckAsync(
                materialRequestId);

        _logger.LogInformation(
            "Inventory check completed for {MaterialRequestId}. " +
            "SufficientStock={SufficientStock}, Shortage={Shortage}",
            materialRequestId,
            inventoryResult.SufficientStock,
            inventoryResult.Shortage);

        if (inventoryResult.SufficientStock)
        {
            _logger.LogInformation(
                "Inventory is sufficient. Workflow finished for {MaterialRequestId}",
                materialRequestId);

            return;
        }

        // Send all eligible vendors to external approval.
        var vendorPayload =
            await _vendorApprovalService.CreateAndSubmitAsync(
                materialRequestId);

        _logger.LogInformation(
            "Vendor approval request submitted for MaterialRequest {MaterialRequestId}. " +
            "VendorCount={VendorCount}. Workflow is waiting for approval.",
            materialRequestId,
            vendorPayload.Items.Count);
    }

    // ============================================================
    // CONTINUE AFTER VENDOR APPROVAL
    // ============================================================

    public async Task ContinueAfterVendorApprovalAsync(
        Guid materialRequestId)
    {
        var request = await _db.MaterialRequests
            .FirstOrDefaultAsync(x => x.Id == materialRequestId)
            ?? throw new InvalidOperationException(
                "Material request not found.");

        if (request.Status != MaterialRequestStatus.SupplierSelected)
        {
            _logger.LogWarning(
                "MaterialRequest {MaterialRequestId} is not SupplierSelected. " +
                "Current status: {Status}",
                materialRequestId,
                request.Status);

            return;
        }

        var existingProcurementRequest =
            await _db.ProcurementRequests
                .FirstOrDefaultAsync(
                    x => x.MaterialRequestId == materialRequestId &&
                         x.Status != ProcurementRequestStatus.Rejected);

        if (existingProcurementRequest is not null)
        {
            _logger.LogInformation(
                "ProcurementRequest {ProcurementRequestId} already exists " +
                "for MaterialRequest {MaterialRequestId}. Skipping duplicate generation.",
                existingProcurementRequest.Id,
                materialRequestId);

            return;
        }

        var procurementRequest =
            await _procurementRequestService.GenerateAsync(
                materialRequestId);

        _logger.LogInformation(
            "ProcurementRequest {ProcurementRequestId} created for " +
            "MaterialRequest {MaterialRequestId}. Waiting for procurement approval.",
            procurementRequest.Id,
            materialRequestId);
    }

    // ============================================================
    // CONTINUE AFTER PROCUREMENT APPROVAL
    // ============================================================

    public async Task ContinueAfterProcurementApprovalAsync(
        Guid procurementRequestId)
    {
        var procurementRequest =
            await _db.ProcurementRequests
                .FirstOrDefaultAsync(
                    x => x.Id == procurementRequestId)
            ?? throw new InvalidOperationException(
                "Procurement request not found.");

        if (procurementRequest.Status !=
            ProcurementRequestStatus.Approved)
        {
            _logger.LogWarning(
                "ProcurementRequest {ProcurementRequestId} is not approved. " +
                "Current status: {Status}",
                procurementRequestId,
                procurementRequest.Status);

            return;
        }

        var existingPo =
            await _db.PurchaseOrders
                .FirstOrDefaultAsync(
                    x => x.ProcurementRequestId == procurementRequestId);

        if (existingPo is not null)
        {
            _logger.LogInformation(
                "PurchaseOrder {PurchaseOrderId} already exists for " +
                "ProcurementRequest {ProcurementRequestId}. Skipping duplicate generation.",
                existingPo.Id,
                procurementRequestId);

            return;
        }

        var purchaseOrder =
            await _purchaseOrderService.GenerateAsync(
                procurementRequestId);

        _logger.LogInformation(
            "PurchaseOrder {PurchaseOrderId} generated for " +
            "ProcurementRequest {ProcurementRequestId}. Waiting for PO approval.",
            purchaseOrder.Id,
            procurementRequestId);
    }

    // ============================================================
    // CONTINUE AFTER PO APPROVAL
    // ============================================================

    public async Task ContinueAfterPurchaseOrderApprovalAsync(
        Guid purchaseOrderId)
    {
        var purchaseOrder =
            await _db.PurchaseOrders
                .FirstOrDefaultAsync(
                    x => x.Id == purchaseOrderId)
            ?? throw new InvalidOperationException(
                "Purchase order not found.");

        if (purchaseOrder.Status != PurchaseOrderStatus.Approved)
        {
            _logger.LogWarning(
                "PurchaseOrder {PurchaseOrderId} is not approved. " +
                "Current status: {Status}",
                purchaseOrderId,
                purchaseOrder.Status);

            return;
        }

        await _deliveryTrackingService.SendOrderAsync(
            purchaseOrderId);

        _logger.LogInformation(
            "PurchaseOrder {PurchaseOrderId} sent to vendor.",
            purchaseOrderId);
    }
}