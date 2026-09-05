using Backend.Contracts;

namespace Backend.Services;

public class FakeProjectDataService : IProjectDataService
{
    public Task<ProjectData> GetProjectDataAsync(Guid projectId)
    {
        var data = new ProjectData(
            projectId,
            BuildType: "Residential",
            Area: 500m,
            StartDate: DateTime.UtcNow.AddDays(30) // project starts 30 days from now, for testing
        );
        return Task.FromResult(data);
    }
}