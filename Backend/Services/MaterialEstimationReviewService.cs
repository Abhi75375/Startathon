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

    // ============================================================
    // STEP A
    // Estimate -> Save DB -> Fetch DB -> Send to frontend
    // ============================================================

    public async Task<MaterialEstimationReview>
        CreateAndSubmitReviewAsync(ProjectData project)
    {
        // 1. Calculate estimation
        var estimates =
            await _estimationService.EstimateAsync(project);

        // 2. Save estimation
        var review = new MaterialEstimationReview
        {
            ProjectId = project.ProjectId,

            Items = estimates
                .Select(e => new MaterialEstimationReviewItem
                {
                    MaterialCode = e.MaterialCode,

                    AiEstimatedQuantity =
                        e.EstimatedQuantity,

                    FinalQuantity =
                        e.EstimatedQuantity,

                    Approved = false
                })
                .ToList()
        };

        _db.MaterialEstimationReviews.Add(review);

        await _db.SaveChangesAsync();

        // 3. Fetch the persisted data from DB
        var savedReview =
            await _db.MaterialEstimationReviews
                .AsNoTracking()
                .Include(r => r.Items)
                .FirstOrDefaultAsync(
                    r => r.Id == review.Id);

        if (savedReview is null)
        {
            throw new InvalidOperationException(
                $"Saved review {review.Id} could not be found.");
        }

        // 4. Build payload from persisted DB data
        var materials =
            savedReview.Items
                .Select(item =>
                    new MaterialEstimationPayload(
                        item.MaterialCode,
                        item.AiEstimatedQuantity))
                .ToList();

        // 5. Send to frontend
        await _frontendGateway.SendMaterialEstimationAsync(
            savedReview.Id,
            savedReview.ProjectId,
            materials);

        return savedReview;
    }


    // ============================================================
    // STEP B
    // Frontend sends approved/edited estimation back
    // ============================================================

    public async Task<List<MaterialRequest>>
        RecordApprovalAsync(
            MaterialEstimationApprovalPayload payload)
    {
        // 1. Find the existing review
        var review =
            await _db.MaterialEstimationReviews
                .Include(r => r.Items)
                .FirstOrDefaultAsync(
                    r => r.Id == payload.ReviewId);

        if (review is null)
        {
            throw new InvalidOperationException(
                $"Material estimation review {payload.ReviewId} was not found.");
        }

        // 2. Validate project ID
        if (review.ProjectId != payload.ProjectId)
        {
            throw new InvalidOperationException(
                "The projectId does not match the material estimation review.");
        }

        var createdRequests =
            new List<MaterialRequest>();

        // 3. Process returned materials
        foreach (var material in payload.Materials)
        {
            if (material.EstimatedQuantity <= 0)
                continue;

            var item =
                review.Items.FirstOrDefault(
                    i => i.MaterialCode ==
                         material.MaterialCode);

            if (item is null)
            {
                continue;
            }

            // ------------------------------------------
            // Update the existing estimation record
            // ------------------------------------------

            item.FinalQuantity =
                material.EstimatedQuantity;

            item.Approved = true;


            // ------------------------------------------
            // Create procurement request
            // ------------------------------------------

            var materialRequest =
                new MaterialRequest
                {
                    MaterialEstimationReviewId =
                        review.Id,

                    MaterialCode =
                        material.MaterialCode,

                    QuantityRequested =
                        material.EstimatedQuantity,

                    RequestedBy =
                        "Supervisor",

                    GeneratedByAi = true,

                    ProjectId =
                        review.ProjectId
                };

            _db.MaterialRequests.Add(
                materialRequest);

            createdRequests.Add(
                materialRequest);
        }

        // 4. Update review status
        review.Status =
            createdRequests.Count > 0
                ? ReviewStatus.Approved
                : ReviewStatus.Rejected;

        review.ReviewedBy =
            "Supervisor";

        review.ReviewedAt =
            DateTime.UtcNow;

        // 5. Persist everything
        await _db.SaveChangesAsync();

        return createdRequests;
    }


    // ============================================================
    // OLD DECISION ENDPOINT
    // Keep this for now if your existing middleware uses it
    // ============================================================

    public async Task<List<MaterialRequest>>
        RecordDecisionAsync(
            Guid reviewId,
            string reviewedBy,
            List<SupervisorDecisionItem> decisions)
    {
        var review =
            await _db.MaterialEstimationReviews
                .Include(r => r.Items)
                .FirstOrDefaultAsync(
                    r => r.Id == reviewId)
            ?? throw new InvalidOperationException(
                "Review not found");

        var createdRequests =
            new List<MaterialRequest>();

        foreach (var decision in decisions)
        {
            var item =
                review.Items.FirstOrDefault(
                    i => i.MaterialCode ==
                         decision.MaterialCode);

            if (item is null)
                continue;

            item.FinalQuantity =
                decision.FinalQuantity;

            item.Approved =
                decision.Approved;

            if (decision.Approved)
            {
                var request =
                    new MaterialRequest
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

                _db.MaterialRequests.Add(
                    request);

                createdRequests.Add(
                    request);
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