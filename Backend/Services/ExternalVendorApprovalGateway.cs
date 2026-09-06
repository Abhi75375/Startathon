using System.Net.Http.Json;
using Backend.Contracts;

namespace Backend.Services;

public class ExternalVendorApprovalGateway : IVendorApprovalGateway
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ExternalVendorApprovalGateway(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SubmitForApprovalAsync(
        VendorApprovalPayload payload)
    {
        var endpoint =
            _configuration["VendorApprovalSettings:SubmitEndpoint"];

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException(
                "VendorApprovalSettings:SubmitEndpoint is not configured.");
        }

        using var response = await _httpClient.PostAsJsonAsync(
            endpoint,
            payload);

        response.EnsureSuccessStatusCode();
    }
}