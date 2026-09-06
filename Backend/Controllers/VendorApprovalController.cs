using Backend.Contracts;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/vendor-approvals")]
public class VendorApprovalController : ControllerBase
{
    private readonly VendorApprovalService _service;
    private readonly IProcurementWorkflowService _workflowService;

    public VendorApprovalController(
        VendorApprovalService service,
        IProcurementWorkflowService workflowService)
    {
        _service = service;
        _workflowService = workflowService;
    }

    // Backend -> external vendor approval system
    [HttpPost("{materialRequestId}/submit")]
    public async Task<IActionResult> Submit(
        Guid materialRequestId)
    {
        try
        {
            var payload =
                await _service.CreateAndSubmitAsync(
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

    // External vendor approval system -> Backend
    [HttpPost("decision")]
    public async Task<IActionResult> ReceiveDecision(
        [FromBody] VendorApprovalDecision decision)
    {
        try
        {
            var approvedBy =
                Request.Headers["X-Approved-By"]
                    .FirstOrDefault()
                ?? "ExternalApprovalSystem";

            var result =
                await _service.RecordApprovalAsync(
                    decision,
                    approvedBy);

            await _workflowService
                .ContinueAfterVendorApprovalAsync(
                    result.Id);

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