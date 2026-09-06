using System.Net.Http.Json;
using Backend.Contracts;

namespace Backend.Services;

public class ExternalVendorApprovalGateway
    : IVendorApprovalGateway
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalVendorApprovalGateway> _logger;

    public ExternalVendorApprovalGateway(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ExternalVendorApprovalGateway> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SubmitForApprovalAsync(
        VendorApprovalPayload payload)
    {
        var endpoint =
            _configuration[
                "VendorApprovalSettings:SubmitEndpoint"];

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException(
                "VendorApprovalSettings:SubmitEndpoint is not configured.");
        }

        _logger.LogInformation(
            "Sending vendor approval for MaterialRequest {MaterialRequestId}. " +
            "VendorCount={VendorCount}",
            payload.MaterialRequestId,
            payload.Items.Count);

        using var response =
            await _httpClient.PostAsJsonAsync(
                endpoint,
                payload);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody =
                await response.Content.ReadAsStringAsync();

            _logger.LogError(
                "Vendor approval endpoint failed. " +
                "Status={StatusCode}, Response={Response}",
                response.StatusCode,
                responseBody);

            throw new HttpRequestException(
                $"Vendor approval endpoint returned " +
                $"{(int)response.StatusCode} ({response.StatusCode}).");
        }
    }
}