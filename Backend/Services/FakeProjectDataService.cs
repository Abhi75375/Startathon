using Backend.Contracts;

namespace Backend.Services;

public class FakeProjectDataService : object
{
    public Task<ProjectData> GetProjectDataAsync(Guid projectId)
    {
        var data = new ProjectData(
            ProjectId: projectId,
            BuildType: "Residential",
            StructureType: "Dispenser Island",
            StructureName: "Sample Dispenser Island",
            AreaSqFt: 450m,
            NumberOfFloors: 1,
            FloorHeightMeters: 3.5m,
            TechnicalSpecifications:
                """{"capacity_liters":25000,"material":"Double-walled steel","dispenser_type":"4-Nozzle MPD"}""",
            StartDate: DateTime.UtcNow.AddDays(30),
            CompletionDate: DateTime.UtcNow.AddDays(90)
        );

        return Task.FromResult(data);
    }
}