namespace Backend.Contracts;

public interface IOrderNotificationGateway
{
    Task SendOrderAsync(OrderNotificationPayload payload);
}

public record OrderNotificationPayload(
    Guid PurchaseOrderId,
    string PoNumber,
    string SupplierId,
    string SupplierName,
    string MaterialCode,
    decimal Quantity,
    decimal TotalCost,
    DateTime EstimatedDeliveryDate
);