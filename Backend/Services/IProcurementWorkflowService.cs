namespace Backend.Services;

public interface IProcurementWorkflowService
{
    Task StartFromMaterialRequestAsync(Guid materialRequestId);

    Task ContinueAfterVendorApprovalAsync(Guid materialRequestId);

    Task ContinueAfterProcurementApprovalAsync(Guid procurementRequestId);

    Task ContinueAfterPurchaseOrderApprovalAsync(Guid purchaseOrderId);
}