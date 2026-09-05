using Backend.Contracts;

namespace Backend.Services;

public class FakePoApprovalGateway : IPoApprovalGateway
{
    public Task SubmitForApprovalAsync(PoApprovalPayload payload)
    {
        Console.WriteLine($"[FAKE] PO {payload.PoNumber} ({payload.PurchaseOrderId}) - " +
            $"{payload.MaterialCode} x{payload.Quantity} from {payload.SupplierName}, ${payload.TotalCost} - sent for PO approval.");
        return Task.CompletedTask;
    }
}