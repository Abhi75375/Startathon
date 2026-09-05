using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api")]
public class ProcurementRequestController : ControllerBase
{
    private readonly ProcurementRequestService _service;
    private readonly IProcurementWorkflowService _workflowService;

    public ProcurementRequestController(
        ProcurementRequestService service,
        IProcurementWorkflowService workflowService)
    {
        _service = service;
        _workflowService = workflowService;
    }

    [HttpPost("material-requests/{id}/generate-procurement-request")]
    public async Task<IActionResult> Generate(Guid id)
    {
        var procurementRequest =
            await _service.GenerateAsync(id);

        return Ok(procurementRequest);
    }

    public record ProcurementDecisionDto(
        string DecidedBy,
        bool Approved,
        string? RejectionReason);

    [HttpPost("procurement-requests/{id}/decision")]
    public async Task<IActionResult> RecordDecision(
        Guid id,
        ProcurementDecisionDto dto)
    {
        try
        {
            var result =
                await _service.RecordDecisionAsync(
                    id,
                    dto.DecidedBy,
                    dto.Approved,
                    dto.RejectionReason);

            // IMPORTANT:
            // If approved, immediately continue the workflow.
            if (dto.Approved)
            {
                await _workflowService
                    .ContinueAfterProcurementApprovalAsync(
                        result.Id);
            }

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