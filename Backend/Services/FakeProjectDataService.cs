using Backend.Contracts;

namespace Backend.Services;

public class FakeProjectDataService : IProjectDataService
{
    public Task<ProjectData> GetProjectDataAsync(Guid projectId)
    {
        // Hardcoded test data until the middleware is ready
        var data = new ProjectData(projectId, BuildType: "Residential", Area: 500m);
        return Task.FromResult(data);
    }
}