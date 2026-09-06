using Backend.Contracts;
using Backend.Models;

namespace Backend.Services;

public class MaterialEstimationService
{
    private readonly IProjectDataService _projectDataService;
    private readonly IHistoricalProjectDataService _historicalDataService;

    // Tuning knobs for when to distrust the algorithm and flag for LLM review
    private const int MinimumSampleSize = 3;
    private const decimal MaxAcceptableVariationRatio = 0.25m; // 25% spread around the mean

    public MaterialEstimationService(
        IProjectDataService projectDataService,
        IHistoricalProjectDataService historicalDataService)
    {
        _projectDataService = projectDataService;
        _historicalDataService = historicalDataService;
    }

    public async Task<List<MaterialEstimate>> EstimateAsync(Guid projectId)
{
    var currentProject =
        await _projectDataService.GetProjectDataAsync(projectId);

    var historicalProjects =
        await _historicalDataService
            .GetHistoricalProjectsAsync(currentProject.BuildType);

    var allMaterialCodes = historicalProjects
        .SelectMany(p => p.MaterialsUsed.Keys)
        .Distinct();

    var effectiveArea =
        currentProject.AreaSqFt *
        Math.Max(currentProject.NumberOfFloors, 1);

    var estimates = new List<MaterialEstimate>();

    foreach (var materialCode in allMaterialCodes)
    {
        var rates = historicalProjects
            .Where(p =>
                p.MaterialsUsed.ContainsKey(materialCode) &&
                p.Area > 0)
            .Select(p =>
                p.MaterialsUsed[materialCode] / p.Area)
            .ToList();

        if (rates.Count == 0)
            continue;

        decimal averageRate = rates.Average();

        decimal estimatedQuantity =
            averageRate * effectiveArea;

        decimal variationRatio =
            CalculateCoefficientOfVariation(
                rates,
                averageRate);

        bool enoughSamples =
            rates.Count >= MinimumSampleSize;

        bool consistentEnough =
            variationRatio <= MaxAcceptableVariationRatio;

        decimal confidenceScore =
            enoughSamples && consistentEnough
                ? 1.0m - variationRatio
                : 0.5m -
                  Math.Min(variationRatio, 0.5m);

        estimates.Add(new MaterialEstimate
        {
            MaterialCode = materialCode,

            EstimatedQuantity =
                Math.Round(
                    estimatedQuantity,
                    2),

            SampleSize = rates.Count,

            ConfidenceScore =
                Math.Round(
                    Math.Clamp(
                        confidenceScore,
                        0m,
                        1m),
                    2),

            NeedsLlmReview =
                !(enoughSamples &&
                  consistentEnough)
        });
    }

    return estimates;
}

    private static decimal CalculateCoefficientOfVariation(List<decimal> values, decimal mean)
    {
        if (mean == 0 || values.Count < 2) return 1.0m; // treat as maximally inconsistent

        decimal variance = values.Sum(v => (v - mean) * (v - mean)) / values.Count;
        decimal stdDev = (decimal)Math.Sqrt((double)variance);

        return stdDev / mean;
    }
}