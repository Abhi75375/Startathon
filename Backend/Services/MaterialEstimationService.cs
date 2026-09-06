using Backend.Contracts;
using Backend.Models;

namespace Backend.Services;

public class MaterialEstimationService
{
    private readonly IHistoricalProjectDataService _historicalDataService;

    private const int MinimumSampleSize = 3;
    private const decimal MaxAcceptableVariationRatio = 0.25m;

    public MaterialEstimationService(
        IHistoricalProjectDataService historicalDataService)
    {
        _historicalDataService = historicalDataService;
    }

    public async Task<List<MaterialEstimate>> EstimateAsync(
        ProjectData currentProject)
    {
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

            var averageRate = rates.Average();

            var estimatedQuantity =
                averageRate * effectiveArea;

            var variationRatio =
                CalculateCoefficientOfVariation(
                    rates,
                    averageRate);

            var enoughSamples =
                rates.Count >= MinimumSampleSize;

            var consistentEnough =
                variationRatio <= MaxAcceptableVariationRatio;

            var confidenceScore =
                enoughSamples && consistentEnough
                    ? 1.0m - variationRatio
                    : 0.5m -
                      Math.Min(variationRatio, 0.5m);

            estimates.Add(new MaterialEstimate
            {
                MaterialCode = materialCode,

                EstimatedQuantity =
                    Math.Round(estimatedQuantity, 2),

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

    private static decimal CalculateCoefficientOfVariation(
        List<decimal> values,
        decimal mean)
    {
        if (mean == 0 || values.Count < 2)
            return 1.0m;

        decimal variance =
            values.Sum(v =>
                (v - mean) * (v - mean))
            / values.Count;

        decimal stdDev =
            (decimal)Math.Sqrt(
                (double)variance);

        return stdDev / mean;
    }
}