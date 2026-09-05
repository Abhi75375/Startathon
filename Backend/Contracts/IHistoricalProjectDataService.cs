namespace Backend.Contracts;

public interface IHistoricalProjectDataService
{
    Task<List<HistoricalProjectRecord>> GetHistoricalProjectsAsync(string buildType);
}

public record HistoricalProjectRecord(
    Guid ProjectId,
    string BuildType,
    decimal Area,
    Dictionary<string, decimal> MaterialsUsed // key = MaterialCode, value = quantity used
);