using Backend.Contracts;

namespace Backend.Services;

public class FakeVendorApprovalGateway
    : IVendorApprovalGateway
{
    private readonly ILogger<FakeVendorApprovalGateway> _logger;

    public FakeVendorApprovalGateway(
        ILogger<FakeVendorApprovalGateway> logger)
    {
        _logger = logger;
    }

    public Task SubmitForApprovalAsync(
        VendorApprovalPayload payload)
    {
        _logger.LogInformation(
            """
            ============================================================
            DEMO VENDOR APPROVAL SENT
            MaterialRequestId: {MaterialRequestId}
            ProjectId: {ProjectId}

            VENDOR OPTIONS
            {Items}
            ============================================================
            """,
            payload.MaterialRequestId,
            payload.ProjectId,
            string.Join(
                Environment.NewLine,
                payload.Items.Select(x =>
                    $"{x.SupplierId} | {x.SupplierName} | " +
                    $"{x.MaterialCode} | Qty={x.Quantity} | " +
                    $"UnitPrice=₹{x.UnitPrice} | " +
                    $"Total=₹{x.TotalAmount} | " +
                    $"Delivery={x.EstimatedDeliveryDate:yyyy-MM-dd}")));

        return Task.CompletedTask;
    }
}