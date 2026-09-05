using Backend.Contracts;

namespace Backend.Services;

public class FakeOrderNotificationGateway : IOrderNotificationGateway
{
    public Task SendOrderAsync(OrderNotificationPayload payload)
    {
        Console.WriteLine($"[FAKE] Order sent to {payload.SupplierName} - " +
            $"PO {payload.PoNumber}: {payload.MaterialCode} x{payload.Quantity}, ${payload.TotalCost}");
        return Task.CompletedTask;
    }
}