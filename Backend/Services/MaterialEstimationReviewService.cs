using Backend.Contracts;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class MaterialEstimationReviewService
{
    private readonly ProcurementDbContext _db;
    private readonly MaterialEstimationService _estimationService;
    private readonly IMaterialEstimationFrontendGateway _frontendGateway;

    public MaterialEstimationReviewService(
        ProcurementDbContext db,
        MaterialEstimationService estimationService,
        IMaterialEstimationFrontendGateway frontendGateway)
    {
        _db = db;
        _estimationService = estimationService;
        _frontendGateway = frontendGateway;
    }

    // Step A:
    // 1. Calculate the estimates
    // 2. Save the estimates to PostgreSQL
    // 3. Fetch the saved review from PostgreSQL
    // 4. Send the saved data to the frontend
    public async Task<MaterialEstimationReview> CreateAndSubmitReviewAsync(
        ProjectData project)
    {
        // -----------------------------------------
        // 1. Calculate estimates
        // -----------------------------------------

        var estimates =
            await _estimationService.EstimateAsync(project);

        // -----------------------------------------
        // 2. Save estimates to database
        // -----------------------------------------

        var review = new MaterialEstimationReview
        {
            ProjectId = project.ProjectId,
            Items = estimates
                .Select(e => new MaterialEstimationReviewItem
                {
                    MaterialCode = e.MaterialCode,
                    AiEstimatedQuantity = e.EstimatedQuantity,
                    FinalQuantity = e.EstimatedQuantity,
                    Approved = false
                })
                .ToList()
        };

        _db.MaterialEstimationReviews.Add(review);

        await _db.SaveChangesAsync();

        // At this point the review and all items
        // definitely exist in PostgreSQL.

        // -----------------------------------------
        // 3. Fetch the saved data from database
        // -----------------------------------------

        var savedReview =
            await _db.MaterialEstimationReviews
                .AsNoTracking()
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == review.Id);

        if (savedReview is null)
        {
            throw new InvalidOperationException(
                $"Saved material estimation review {review.Id} could not be found.");
        }

        // -----------------------------------------
        // 4. Build frontend payload from DB data
        // -----------------------------------------

        var materials = savedReview.Items
            .Select(item => new MaterialEstimationPayload(
                item.MaterialCode,
                item.AiEstimatedQuantity))
            .ToList();

        // -----------------------------------------
        // 5. Send saved DB data to frontend
        // -----------------------------------------

        await _frontendGateway.SendMaterialEstimationAsync(
            savedReview.Id,
            savedReview.ProjectId,
            materials);

        return savedReview;
    }

    // Step B:
    // Middleware/frontend sends supervisor's decision
    public async Task<List<MaterialRequest>> RecordDecisionAsync(
        Guid reviewId,
        string reviewedBy,
        List<SupervisorDecisionItem> decisions)
    {
        var review =
            await _db.MaterialEstimationReviews
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == reviewId)
                ?? throw new InvalidOperationException(
                    "Review not found");

        var createdRequests =
            new List<MaterialRequest>();

        foreach (var decision in decisions)
        {
            var item = review.Items
                .FirstOrDefault(i =>
                    i.MaterialCode == decision.MaterialCode);

            if (item is null)
                continue;

            item.FinalQuantity =
                decision.FinalQuantity;

            item.Approved =
                decision.Approved;

            if (decision.Approved)
            {
                var request = new MaterialRequest
                {
                    MaterialEstimationReviewId =
                        review.Id,

                    MaterialCode =
                        item.MaterialCode,

                    QuantityRequested =
                        decision.FinalQuantity,

                    RequestedBy =
                        reviewedBy,

                    GeneratedByAi = true,

                    ProjectId =
                        review.ProjectId
                };

                _db.MaterialRequests.Add(request);

                createdRequests.Add(request);
            }
        }

        review.Status =
            decisions.Any(d => d.Approved)
                ? ReviewStatus.Approved
                : ReviewStatus.Rejected;

        review.ReviewedBy =
            reviewedBy;

        review.ReviewedAt =
            DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return createdRequests;
    }
}

public record SupervisorDecisionItem(
    string MaterialCode,
    decimal FinalQuantity,
    bool Approved);