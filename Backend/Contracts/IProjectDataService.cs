namespace Backend.Contracts;

public interface IProjectDataService
{
    Task<ProjectData> GetProjectDataAsync(Guid projectId);
}

public record ProjectData(
    Guid ProjectId,
    string BuildType,   // e.g. "Residential", "Warehouse"
    decimal Area,        // in square meters
    DateTime StartDate
);