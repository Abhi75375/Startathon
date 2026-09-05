using Backend.Contracts;

namespace Backend.Services;

public class FakeProcurementApprovalGateway : IProcurementApprovalGateway
{
    public Task SubmitForApprovalAsync(ProcurementApprovalPayload payload)
    {
        Console.WriteLine($"[FAKE] Procurement request {payload.ProcurementRequestId} " +
            $"({payload.MaterialCode} x{payload.Quantity}, ${payload.TotalCost}) sent to middleware for approval.");
        return Task.CompletedTask;
    }
}