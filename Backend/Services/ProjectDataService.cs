using Backend.Contracts;
using System.Net.Http.Json;

namespace Backend.Services;

public class ProjectDataService : IProjectDataService
{
    private readonly HttpClient _httpClient;

    public ProjectDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProjectData> GetProjectDataAsync(Guid projectId)
    {
        var response = await _httpClient.GetAsync(
            $"projects/{projectId}/structure"
        );

        response.EnsureSuccessStatusCode();

        var structure = await response.Content.ReadFromJsonAsync<StructureApiResponse>();

        if (structure is null)
            throw new InvalidOperationException(
                $"No structure data returned for project {projectId}"
            );

        return new ProjectData(
            structure.ProjectId,
            structure.StructureType,
            structure.StructureType,
            structure.StructureName,
            structure.AreaSqFt,
            structure.NumberOfFloors,
            structure.FloorHeightMeters,
            structure.TechnicalSpecifications,
            structure.StartDate,
            structure.CompletionDate
        );
    }

    private sealed record StructureApiResponse(
        Guid ProjectId,
        string StructureName,
        string StructureType,
        decimal AreaSqFt,
        int NumberOfFloors,
        decimal FloorHeightMeters,
        decimal AllocatedBudget,
        string Status,
        DateTime? StartDate,
        DateTime? CompletionDate,
        string? TechnicalSpecifications,
        string? Notes
    );
}