using Backend.Contracts;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/material-estimation")]
public class MaterialEstimationApprovalController : ControllerBase
{
    private readonly MaterialEstimationReviewService _service;

    public MaterialEstimationApprovalController(
        MaterialEstimationReviewService service)
    {
        _service = service;
    }

    [HttpPost("approval")]
    public async Task<IActionResult> ReceiveApproval(
        [FromBody] MaterialEstimationApprovalPayload payload)
    {
        try
        {
            var materialRequests =
                await _service.RecordApprovalAsync(payload);

            return Ok(new
            {
                success = true,
                reviewId = payload.ReviewId,
                projectId = payload.ProjectId,
                materialRequests
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}