using Backend.Contracts;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/vendor-approvals")]
public class VendorApprovalController : ControllerBase
{
    private readonly VendorApprovalService _service;

    public VendorApprovalController(VendorApprovalService service)
    {
        _service = service;
    }

    // Backend -> external approval/display system
    [HttpPost("{materialRequestId}/submit")]
    public async Task<IActionResult> Submit(Guid materialRequestId)
    {
        try
        {
            var payload = await _service.CreateAndSubmitAsync(
                materialRequestId);

            return Ok(payload);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    // External approval/display system -> Backend
    [HttpPost("decision")]
    public async Task<IActionResult> ReceiveDecision(
        [FromBody] VendorApprovalDecision decision)
    {
        try
        {
            var approvedBy =
                Request.Headers["X-Approved-By"].FirstOrDefault()
                ?? "ExternalApprovalSystem";

            var result = await _service.RecordApprovalAsync(
                decision,
                approvedBy);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }
}