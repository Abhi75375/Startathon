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

    // Step 4 - guarded entry point, only usable when a request is genuinely awaiting first selection
    public async Task<SupplierSelectionResult> SelectSupplierAsync(Guid materialRequestId)
    {
        var request = await _db.MaterialRequests.FindAsync(materialRequestId)
            ?? throw new InvalidOperationException("Request not found");

        if (request.Status != MaterialRequestStatus.ShortageIdentified
            && request.Status != MaterialRequestStatus.NoSupplierAvailable)
            throw new InvalidOperationException(
                $"Request must be in ShortageIdentified or NoSupplierAvailable status to select a supplier. Current status: {request.Status}");

        var (winner, reason) = await FindBestOfferAsync(request);

        if (winner is null)
        {
            request.Status = MaterialRequestStatus.NoSupplierAvailable;
            await _db.SaveChangesAsync();
            return new SupplierSelectionResult(request.Id, Success: false, SelectedSupplier: null, Reason: reason);
        }

        ApplyWinnerToRequest(request, winner);
        request.Status = MaterialRequestStatus.SupplierSelected;
        await _db.SaveChangesAsync();

        return new SupplierSelectionResult(request.Id, Success: true, SelectedSupplier: winner, Reason: null);
    }

    // NEW - used by the escalation path (vendor declined mid-order) - no status guard, since the
    // request may already be past SupplierSelected by the time a vendor confirmation fails
    public async Task<(SupplierOffer? Offer, string? Reason)> FindNextBestOfferAsync(Guid materialRequestId)
    {
        var request = await _db.MaterialRequests.FindAsync(materialRequestId)
            ?? throw new InvalidOperationException("Request not found");

        return await FindBestOfferAsync(request);
    }

    private async Task<(SupplierOffer? Offer, string? Reason)> FindBestOfferAsync(MaterialRequest request)
    {
        DateTime? deadline = null;
        if (request.ProjectId.HasValue)
        {
            var project = await _projectDataService.GetProjectDataAsync(request.ProjectId.Value);
            deadline = project.StartDate;
        }

        var allOffers = await _supplierService.GetSuppliersAsync(request.MaterialCode);
        var budget = await _budgetService.GetMaxBudgetAsync(request.MaterialCode);

        var excludedIds = (request.ExcludedSupplierIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet();

        var notExcluded = allOffers.Where(o => !excludedIds.Contains(o.SupplierId)).ToList();

        var afterDeadlineFilter = deadline.HasValue
            ? notExcluded.Where(o => o.DeliveryDate <= deadline.Value).ToList()
            : notExcluded;

        var eligibleOffers = afterDeadlineFilter
            .Where(o => o.PricePerUnit <= budget.MaxPricePerUnit)
            .ToList();

        if (eligibleOffers.Count == 0)
            return (null, "No remaining vendor met both the delivery deadline and budget constraints.");

        decimal minPrice = eligibleOffers.Min(o => o.PricePerUnit);
        decimal maxPrice = eligibleOffers.Max(o => o.PricePerUnit);

        var scored = eligibleOffers.Select(o =>
        {
            decimal priceScore = maxPrice == minPrice ? 1.0m : (maxPrice - o.PricePerUnit) / (maxPrice - minPrice);
            decimal totalScore = (priceScore * PriceWeight) + (o.ReliabilityScore * ReliabilityWeight) + ((o.Rating / 5.0m) * RatingWeight);
            return (Offer: o, Score: totalScore);
        })
        .OrderByDescending(x => x.Score)
        .ToList();

        return (scored.First().Offer, null);
    }

    private static void ApplyWinnerToRequest(MaterialRequest request, SupplierOffer winner)
    {
        request.SelectedSupplierId = winner.SupplierId;
        request.SelectedSupplierName = winner.SupplierName;
        request.SelectedSupplierPrice = winner.PricePerUnit;
        request.SelectedSupplierTelegramChatId = winner.TelegramChatId;
        request.EstimatedDeliveryDate = winner.DeliveryDate;
    }
}

public record SupplierSelectionResult(Guid MaterialRequestId, bool Success, SupplierOffer? SelectedSupplier, string? Reason);