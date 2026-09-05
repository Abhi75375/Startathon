namespace Backend.Contracts;

public interface IPoApprovalGateway
{
    Task SubmitForApprovalAsync(PoApprovalPayload payload);
}

public record PoApprovalPayload(
    Guid PurchaseOrderId,
    string PoNumber,
    string MaterialCode,
    decimal Quantity,
    string SupplierName,
    decimal UnitPrice,
    decimal TotalCost,
    DateTime EstimatedDeliveryDate
);