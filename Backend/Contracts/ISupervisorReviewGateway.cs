namespace Backend.Contracts;

public interface ISupervisorReviewGateway
{
    // Sends the estimation review to the middleware so a supervisor can see/edit/approve it there.
    Task SubmitForReviewAsync(Guid reviewId, Guid projectId, List<ReviewItemPayload> items);
}

public record ReviewItemPayload(string MaterialCode, decimal AiEstimatedQuantity);