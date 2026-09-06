namespace Backend.Contracts;

public interface IMaterialEstimationFrontendGateway
{
    Task SendMaterialEstimationAsync(
        Guid reviewId,
        Guid projectId,
        List<MaterialEstimationPayload> materials);
}

public record MaterialEstimationPayload(
    string MaterialCode,
    decimal EstimatedQuantity);