using Backend.Contracts;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/material-estimation-reviews")]
public class MaterialEstimationReviewController : ControllerBase
{
    private readonly MaterialEstimationReviewService _service;

    public MaterialEstimationReviewController(
        MaterialEstimationReviewService service)
    {
        _service = service;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitForReview(
        [FromBody] ProjectData project)
    {
        var review = await _service.CreateAndSubmitReviewAsync(project);

        return Ok(review);
    }

    public record DecisionDto(
        string ReviewedBy,
        List<SupervisorDecisionItem> Decisions);

    [HttpPost("{reviewId}/decision")]
    public async Task<IActionResult> RecordDecision(
        Guid reviewId,
        [FromBody] DecisionDto dto)
    {
        var createdRequests = await _service.RecordDecisionAsync(
            reviewId,
            dto.ReviewedBy,
            dto.Decisions);

        return Ok(createdRequests);
    }
}