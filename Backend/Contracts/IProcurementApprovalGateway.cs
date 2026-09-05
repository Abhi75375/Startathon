namespace Backend.Contracts;

public interface IProcurementApprovalGateway
{
    Task SubmitForApprovalAsync(ProcurementApprovalPayload payload);
}

public record ProcurementApprovalPayload(
    Guid ProcurementRequestId,
    string MaterialCode,
    decimal Quantity,
    string SupplierName,
    decimal UnitPrice,
    decimal TotalCost,
    DateTime EstimatedDeliveryDate
);