namespace Backend.Contracts;

public interface IVendorApprovalGateway
{
    Task SubmitForApprovalAsync(VendorApprovalPayload payload);
}

public record VendorApprovalPayload(
    Guid MaterialRequestId,
    Guid? ProjectId,
    List<VendorApprovalItem> Items
);

public record VendorApprovalItem(
    string MaterialCode,
    decimal Quantity,
    string SupplierId,
    string SupplierName,
    decimal UnitPrice,
    decimal TotalAmount,
    DateTime EstimatedDeliveryDate
);

public record VendorApprovalDecision(
    Guid MaterialRequestId,
    Guid? ProjectId,
    List<VendorApprovalItem> Items
);