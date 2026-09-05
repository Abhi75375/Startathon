using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/material-estimation-reviews")]
public class MaterialEstimationReviewController : ControllerBase
{
    private readonly MaterialEstimationReviewService _service;

    public MaterialEstimationReviewController(MaterialEstimationReviewService service)
    {
        _service = service;
    }

    public record SubmitReviewDto(Guid ProjectId);

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitForReview(SubmitReviewDto dto)
    {
        var review = await _service.CreateAndSubmitReviewAsync(dto.ProjectId);
        return Ok(review);
    }

    public record DecisionDto(string ReviewedBy, List<SupervisorDecisionItem> Decisions);

    [HttpPost("{reviewId}/decision")]
    public async Task<IActionResult> RecordDecision(Guid reviewId, DecisionDto dto)
    {
        var createdRequests = await _service.RecordDecisionAsync(reviewId, dto.ReviewedBy, dto.Decisions);
        return Ok(createdRequests);
    }
}