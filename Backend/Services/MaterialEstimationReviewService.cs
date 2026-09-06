using Backend.Contracts;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class MaterialEstimationReviewService
{
    private readonly ProcurementDbContext _db;
    private readonly MaterialEstimationService _estimationService;
    private readonly ISupervisorReviewGateway _reviewGateway;

    public MaterialEstimationReviewService(
        ProcurementDbContext db,
        MaterialEstimationService estimationService,
        ISupervisorReviewGateway reviewGateway)
    {
        _db = db;
        _estimationService = estimationService;
        _reviewGateway = reviewGateway;
    }

    // Step A: estimate materials, save as a pending review, send to middleware
    public async Task<MaterialEstimationReview> CreateAndSubmitReviewAsync(ProjectData project)
    {
        var estimates = await _estimationService.EstimateAsync(project);

        var review = new MaterialEstimationReview
        {
            ProjectId = project.ProjectId,
            Items = estimates.Select(e => new MaterialEstimationReviewItem
            {
                MaterialCode = e.MaterialCode,
                AiEstimatedQuantity = e.EstimatedQuantity,
                FinalQuantity = e.EstimatedQuantity
            }).ToList()
        };

        _db.MaterialEstimationReviews.Add(review);
        await _db.SaveChangesAsync();

        var payload = review.Items
            .Select(i => new ReviewItemPayload(
                i.MaterialCode,
                i.AiEstimatedQuantity))
            .ToList();

        await _reviewGateway.SubmitForReviewAsync(
            review.Id,
            project.ProjectId,
            payload);

        return review;
    }

    // Step B: middleware calls this back with the supervisor's decision
    public async Task<List<MaterialRequest>> RecordDecisionAsync(
        Guid reviewId,
        string reviewedBy,
        List<SupervisorDecisionItem> decisions)
    {
        var review = await _db.MaterialEstimationReviews
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == reviewId)
            ?? throw new InvalidOperationException("Review not found");

        var createdRequests = new List<MaterialRequest>();

        foreach (var decision in decisions)
        {
            var item = review.Items
                .FirstOrDefault(i => i.MaterialCode == decision.MaterialCode);

            if (item is null)
                continue;

            item.FinalQuantity = decision.FinalQuantity;
            item.Approved = decision.Approved;

            if (decision.Approved)
            {
                var request = new MaterialRequest
                {
                    MaterialEstimationReviewId = review.Id,
                    MaterialCode = item.MaterialCode,
                    QuantityRequested = decision.FinalQuantity,
                    RequestedBy = reviewedBy,
                    GeneratedByAi = true,
                    ProjectId = review.ProjectId
                };

                _db.MaterialRequests.Add(request);
                createdRequests.Add(request);
            }
        }

        review.Status = decisions.Any(d => d.Approved)
            ? ReviewStatus.Approved
            : ReviewStatus.Rejected;

        review.ReviewedBy = reviewedBy;
        review.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return createdRequests;
    }
}

public record SupervisorDecisionItem(
    string MaterialCode,
    decimal FinalQuantity,
    bool Approved);