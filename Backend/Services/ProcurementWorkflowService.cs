using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ProcurementWorkflowService : IProcurementWorkflowService
{
    private readonly ProcurementDbContext _db;
    private readonly InventoryCheckService _inventoryCheckService;
    private readonly SupplierSelectionService _supplierSelectionService;
    private readonly ProcurementRequestService _procurementRequestService;
    private readonly PurchaseOrderService _purchaseOrderService;
    private readonly DeliveryTrackingService _deliveryTrackingService;
    private readonly ILogger<ProcurementWorkflowService> _logger;

    public ProcurementWorkflowService(
    ProcurementDbContext db,
    InventoryCheckService inventoryCheckService,
    SupplierSelectionService supplierSelectionService,
    ProcurementRequestService procurementRequestService,
    PurchaseOrderService purchaseOrderService,
    DeliveryTrackingService deliveryTrackingService,
    ILogger<ProcurementWorkflowService> logger)
    {
        _db = db;
        _inventoryCheckService = inventoryCheckService;
        _supplierSelectionService = supplierSelectionService;
        _procurementRequestService = procurementRequestService;
        _purchaseOrderService = purchaseOrderService;
        _logger = logger;
        _deliveryTrackingService = deliveryTrackingService;
    }

    // ============================================================
    // START WORKFLOW
    //
    // MaterialRequest already exists.
    //
    // Created
    //    ↓
    // Inventory Check
    //    ↓
    // Shortage?
    //    ↓ yes
    // Supplier Selection
    //    ↓
    // Procurement Request
    //    ↓
    // WAIT FOR HUMAN APPROVAL
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

        // --------------------------------------------------------
        // STEP 2: INVENTORY CHECK
        // --------------------------------------------------------

        var inventoryResult =
            await _inventoryCheckService.CheckAsync(
                materialRequestId);

        _logger.LogInformation(
            "Inventory check completed for {MaterialRequestId}. " +
            "SufficientStock={SufficientStock}, Shortage={Shortage}",
            materialRequestId,
            inventoryResult.SufficientStock,
            inventoryResult.Shortage);

        // --------------------------------------------------------
        // If there is enough stock, procurement is not required.
        // --------------------------------------------------------

        if (inventoryResult.SufficientStock)
        {
            _logger.LogInformation(
                "Inventory is sufficient. Procurement workflow finished for {MaterialRequestId}",
                materialRequestId);

            return;
        }

        // --------------------------------------------------------
        // STEP 4: SUPPLIER SELECTION
        // --------------------------------------------------------

        var supplierResult =
            await _supplierSelectionService.SelectSupplierAsync(
                materialRequestId);

        if (!supplierResult.Success)
        {
            _logger.LogWarning(
                "No supplier available for MaterialRequest {MaterialRequestId}. Reason: {Reason}",
                materialRequestId,
                supplierResult.Reason);

            return;
        }

        _logger.LogInformation(
            "Supplier selected for {MaterialRequestId}: {SupplierId} - {SupplierName}",
            materialRequestId,
            supplierResult.SelectedSupplier?.SupplierId,
            supplierResult.SelectedSupplier?.SupplierName);

        // --------------------------------------------------------
        // STEP 5: GENERATE PROCUREMENT REQUEST
        //
        // This also submits the request to the approval gateway.
        // --------------------------------------------------------

        var procurementRequest =
            await _procurementRequestService.GenerateAsync(
                materialRequestId);

        _logger.LogInformation(
            "Procurement request {ProcurementRequestId} created " +
            "for MaterialRequest {MaterialRequestId}. " +
            "Workflow is now waiting for human approval.",
            procurementRequest.Id,
            materialRequestId);

        // --------------------------------------------------------
        // STOP HERE.
        //
        // Human procurement approval is required.
        // The approval callback will resume the workflow.
        // --------------------------------------------------------
    }


    // ============================================================
    // CONTINUE AFTER PROCUREMENT APPROVAL
    //
    // Approved ProcurementRequest
    //            ↓
    //       Generate PO
    //            ↓
    //       PO Approval
    //            ↓
    //          WAIT
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

        // Only continue when actually approved.
        if (procurementRequest.Status !=
            ProcurementRequestStatus.Approved)
        {
            _logger.LogWarning(
                "Procurement request {ProcurementRequestId} " +
                "is not approved. Current status: {Status}",
                procurementRequestId,
                procurementRequest.Status);

            return;
        }

        // --------------------------------------------------------
        // SAFETY:
        // Do not create a second PO if this workflow callback
        // happens twice.
        // --------------------------------------------------------

        var existingPo =
            await _db.PurchaseOrders
                .FirstOrDefaultAsync(
                    x => x.ProcurementRequestId ==
                         procurementRequestId);

        if (existingPo is not null)
        {
            _logger.LogInformation(
                "PO {PurchaseOrderId} already exists for " +
                "ProcurementRequest {ProcurementRequestId}. " +
                "Skipping duplicate generation.",
                existingPo.Id,
                procurementRequestId);

            return;
        }

        // --------------------------------------------------------
        // STEP 7: GENERATE PURCHASE ORDER
        // --------------------------------------------------------

        var purchaseOrder =
            await _purchaseOrderService.GenerateAsync(
                procurementRequestId);

        _logger.LogInformation(
            "Purchase Order {PurchaseOrderId} generated " +
            "for ProcurementRequest {ProcurementRequestId}. " +
            "Workflow is now waiting for PO approval.",
            purchaseOrder.Id,
            procurementRequestId);

        // --------------------------------------------------------
        // STOP HERE.
        //
        // Human PO approval is required.
        // The PO approval callback will resume the workflow.
        // --------------------------------------------------------
    }


    // ============================================================
    // CONTINUE AFTER PO APPROVAL
    // ============================================================

    public async Task ContinueAfterPurchaseOrderApprovalAsync(Guid purchaseOrderId)
    {
        var purchaseOrder =
            await _db.PurchaseOrders
                .FirstOrDefaultAsync(x => x.Id == purchaseOrderId)
            ?? throw new InvalidOperationException("Purchase order not found.");

        if (purchaseOrder.Status != PurchaseOrderStatus.Approved)
        {
            _logger.LogWarning(
                "Purchase order {PurchaseOrderId} is not approved. " +
                "Current status: {Status}",
                purchaseOrderId,
                purchaseOrder.Status);

            return;
        }

        _logger.LogInformation(
            "Purchase Order {PurchaseOrderId} approved. " +
            "Sending order to vendor.",
            purchaseOrderId);

        await _deliveryTrackingService.SendOrderAsync(purchaseOrderId);

        _logger.LogInformation(
            "Purchase Order {PurchaseOrderId} sent to vendor. " +
            "Workflow is now waiting for vendor confirmation.",
            purchaseOrderId);
    }
}