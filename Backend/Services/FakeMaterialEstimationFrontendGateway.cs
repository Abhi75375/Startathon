using Backend.Contracts;

namespace Backend.Services;

public class FakeMaterialEstimationFrontendGateway
    : IMaterialEstimationFrontendGateway
{
    private readonly ILogger<FakeMaterialEstimationFrontendGateway> _logger;

    public FakeMaterialEstimationFrontendGateway(
        ILogger<FakeMaterialEstimationFrontendGateway> logger)
    {
        _logger = logger;
    }

    public Task SendMaterialEstimationAsync(
        Guid reviewId,
        Guid projectId,
        List<MaterialEstimationPayload> materials)
    {
        _logger.LogInformation(
            "==================================================");

        _logger.LogInformation(
            "MATERIAL ESTIMATION REVIEW SUBMITTED");

        _logger.LogInformation(
            "Review ID: {ReviewId}",
            reviewId);

        _logger.LogInformation(
            "Project ID: {ProjectId}",
            projectId);

        foreach (var material in materials)
        {
            _logger.LogInformation(
                "Material: {MaterialCode} | Estimated Quantity: {Quantity}",
                material.MaterialCode,
                material.EstimatedQuantity);
        }

        _logger.LogInformation(
            "==================================================");

        return Task.CompletedTask;
    }
}