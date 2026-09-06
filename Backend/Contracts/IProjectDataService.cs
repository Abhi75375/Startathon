namespace Backend.Contracts;

public interface IProjectDataService
{
    Task<ProjectData> GetProjectDataAsync(Guid projectId);
}

public record ProjectData(
    Guid ProjectId,
    string BuildType,
    string StructureType,
    string StructureName,
    decimal AreaSqFt,
    int NumberOfFloors,
    decimal FloorHeightMeters,
    string? TechnicalSpecifications,
    DateTime? StartDate,
    DateTime? CompletionDate
);