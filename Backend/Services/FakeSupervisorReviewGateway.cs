using Backend.Contracts;

namespace Backend.Services;

public class FakeSupervisorReviewGateway : ISupervisorReviewGateway
{
    public Task SubmitForReviewAsync(Guid reviewId, Guid projectId, List<ReviewItemPayload> items)
    {
        // No-op for local testing — in real life this would notify the middleware.
        // You'll manually call the decision callback endpoint yourself during testing
        // to simulate "the supervisor responded."
        Console.WriteLine($"[FAKE] Review {reviewId} for project {projectId} sent to middleware with {items.Count} materials.");
        return Task.CompletedTask;
    }
}