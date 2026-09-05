using Backend.Contracts;

namespace Backend.Services;

public class FakeHistoricalProjectDataService : IHistoricalProjectDataService
{
    public Task<List<HistoricalProjectRecord>> GetHistoricalProjectsAsync(string buildType)
    {
        // Hardcoded past "Residential" projects with materials used, for local testing.
        // Each project has a different area, so rates (quantity/area) can vary naturally.
        var records = new List<HistoricalProjectRecord>
        {
            new(Guid.NewGuid(), "Residential", 400m, new Dictionary<string, decimal>
            {
                ["CEMENT-001"] = 320m,
                ["STEEL-001"] = 1800m,
                ["BRICK-001"] = 24000m
            }),
            new(Guid.NewGuid(), "Residential", 600m, new Dictionary<string, decimal>
            {
                ["CEMENT-001"] = 465m,
                ["STEEL-001"] = 2750m,
                ["BRICK-001"] = 36500m
            }),
            new(Guid.NewGuid(), "Residential", 550m, new Dictionary<string, decimal>
            {
                ["CEMENT-001"] = 440m,
                ["STEEL-001"] = 2530m,
                ["BRICK-001"] = 33200m
            }),
            new(Guid.NewGuid(), "Residential", 480m, new Dictionary<string, decimal>
            {
                ["CEMENT-001"] = 384m,
                ["STEEL-001"] = 2170m,
                ["BRICK-001"] = 28900m
            })
        };

        var filtered = records.Where(r => r.BuildType == buildType).ToList();
        return Task.FromResult(filtered);
    }
}