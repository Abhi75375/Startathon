using Backend.Contracts;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/material-estimation-reviews")]
public class MaterialEstimationReviewController : ControllerBase
{
    private readonly MaterialEstimationReviewService _service;
    private readonly IProcurementWorkflowService _workflowService;

    public MaterialEstimationReviewController(
        MaterialEstimationReviewService service,
        IProcurementWorkflowService workflowService)
    {
        _service = service;
        _workflowService = workflowService;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitForReview(
        [FromBody] ProjectData project)
    {
        var review =
            await _service.CreateAndSubmitReviewAsync(
                project);

        return Ok(review);
    }

    [HttpPost("{reviewId}/decision")]
    public async Task<IActionResult> RecordDecision(
        Guid reviewId,
        [FromBody] DecisionDto dto)
    {
        var createdRequests =
            await _service.RecordDecisionAsync(
                reviewId,
                dto.ReviewedBy,
                dto.Decisions);

        foreach (var request in createdRequests)
        {
            await _workflowService
                .StartFromMaterialRequestAsync(
                    request.Id);
        }

        return Ok(createdRequests);
    }

    public record DecisionDto(
        string ReviewedBy,
        List<SupervisorDecisionItem> Decisions);
}