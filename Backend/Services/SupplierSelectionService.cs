using Backend.Contracts;
using Backend.Data;
using Backend.Models;

namespace Backend.Services;

public class SupplierSelectionService
{
    private readonly ProcurementDbContext _db;
    private readonly ISupplierService _supplierService;
    private readonly IBudgetService _budgetService;
    private readonly IProjectDataService _projectDataService;

    // Weighting for the ranking score, once hard filters have narrowed the field
    private const decimal PriceWeight = 0.5m;
    private const decimal ReliabilityWeight = 0.3m;
    private const decimal RatingWeight = 0.2m;

    public SupplierSelectionService(
        ProcurementDbContext db,
        ISupplierService supplierService,
        IBudgetService budgetService,
        IProjectDataService projectDataService)
    {
        _db = db;
        _supplierService = supplierService;
        _budgetService = budgetService;
        _projectDataService = projectDataService;
    }

    public async Task<SupplierSelectionResult> SelectSupplierAsync(Guid materialRequestId)
    {
        var request = await _db.MaterialRequests.FindAsync(materialRequestId)
            ?? throw new InvalidOperationException("Request not found");

        if (request.Status != MaterialRequestStatus.ShortageIdentified)
            throw new InvalidOperationException(
                $"Request must be in ShortageIdentified status to select a supplier. Current status: {request.Status}");

        // Deadline: only enforced if this request is linked to a real project
        DateTime? deadline = null;
        if (request.ProjectId.HasValue)
        {
            var project = await _projectDataService.GetProjectDataAsync(request.ProjectId.Value);
            deadline = project.StartDate;
        }

        var allOffers = await _supplierService.GetSuppliersAsync(request.MaterialCode);
        var budget = await _budgetService.GetMaxBudgetAsync(request.MaterialCode);

        // Hard filter 1: delivery must land before the project's start date (if we have one)
        var afterDeadlineFilter = deadline.HasValue
            ? allOffers.Where(o => o.DeliveryDate <= deadline.Value).ToList()
            : allOffers;

        // Hard filter 2: price must not exceed the middleware's max budget for this material
        var eligibleOffers = afterDeadlineFilter
            .Where(o => o.PricePerUnit <= budget.MaxPricePerUnit)
            .ToList();

        if (eligibleOffers.Count == 0)
        {
            request.Status = MaterialRequestStatus.NoSupplierAvailable;
            await _db.SaveChangesAsync();
            return new SupplierSelectionResult(request.Id, Success: false, SelectedSupplier: null,
                Reason: "No supplier met both the delivery deadline and budget constraints.");
        }

        // Score the survivors: normalize each factor to 0-1, then combine with weights
        decimal minPrice = eligibleOffers.Min(o => o.PricePerUnit);
        decimal maxPrice = eligibleOffers.Max(o => o.PricePerUnit);

        var scored = eligibleOffers.Select(o =>
        {
            // Lower price is better, so invert the normalization; guard against all-equal prices
            decimal priceScore = maxPrice == minPrice ? 1.0m : (maxPrice - o.PricePerUnit) / (maxPrice - minPrice);
            decimal reliabilityScore = o.ReliabilityScore; // already 0-1
            decimal ratingScore = o.Rating / 5.0m;          // normalize 0-5 to 0-1

            decimal totalScore = (priceScore * PriceWeight)
                                + (reliabilityScore * ReliabilityWeight)
                                + (ratingScore * RatingWeight);

            return (Offer: o, Score: totalScore);
        })
        .OrderByDescending(x => x.Score)
        .ToList();

        var winner = scored.First().Offer;

        request.Status = MaterialRequestStatus.SupplierSelected;
        request.SelectedSupplierId = winner.SupplierId;
        request.SelectedSupplierName = winner.SupplierName;
        request.SelectedSupplierPrice = winner.PricePerUnit;
        request.EstimatedDeliveryDate = winner.DeliveryDate;

        await _db.SaveChangesAsync();

        return new SupplierSelectionResult(request.Id, Success: true, SelectedSupplier: winner, Reason: null);
    }
}

public record SupplierSelectionResult(
    Guid MaterialRequestId,
    bool Success,
    SupplierOffer? SelectedSupplier,
    string? Reason
);