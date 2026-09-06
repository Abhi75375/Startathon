using System.Net.Http.Json;
using Backend.Contracts;

namespace Backend.Services;

public class MaterialEstimationFrontendGateway
    : IMaterialEstimationFrontendGateway
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MaterialEstimationFrontendGateway> _logger;

    public MaterialEstimationFrontendGateway(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MaterialEstimationFrontendGateway> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendMaterialEstimationAsync(
        Guid reviewId,
        Guid projectId,
        List<MaterialEstimationPayload> materials)
    {
        var endpoint =
            _configuration["FrontendSettings:MaterialEstimationEndpoint"];

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException(
                "FrontendSettings:MaterialEstimationEndpoint is not configured.");
        }

        var payload = new
        {
            reviewId,
            projectId,
            materials
        };

        _logger.LogInformation(
            "Sending material estimation review {ReviewId} for project {ProjectId} to frontend. Material count: {Count}",
            reviewId,
            projectId,
            materials.Count);

        var response = await _httpClient.PostAsJsonAsync(
            endpoint,
            payload);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogError(
                "Frontend material estimation request failed. Status: {StatusCode}. Response: {Response}",
                response.StatusCode,
                responseBody);

            throw new HttpRequestException(
                $"Frontend endpoint returned {(int)response.StatusCode} ({response.StatusCode}).");
        }

        _logger.LogInformation(
            "Material estimation successfully sent to frontend for review {ReviewId}.",
            reviewId);
    }
}