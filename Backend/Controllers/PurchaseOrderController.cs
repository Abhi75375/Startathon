using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api")]
public class PurchaseOrderController : ControllerBase
{
    private readonly PurchaseOrderService _service;
    private readonly IProcurementWorkflowService _workflowService;

    public PurchaseOrderController(
        PurchaseOrderService service,
        IProcurementWorkflowService workflowService)
    {
        _service = service;
        _workflowService = workflowService;
    }

    [HttpPost("procurement-requests/{procurementRequestId}/generate-po")]
    public async Task<IActionResult> Generate(
        Guid procurementRequestId)
    {
        try
        {
            var po =
                await _service.GenerateAsync(
                    procurementRequestId);

            return Ok(po);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    public record PoDecisionDto(
        string DecidedBy,
        bool Approved,
        string? RejectionReason);

    [HttpPost("purchase-orders/{id}/decision")]
    public async Task<IActionResult> RecordDecision(
        Guid id,
        PoDecisionDto dto)
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
            // If approved, immediately continue.
            if (dto.Approved)
            {
                await _workflowService
                    .ContinueAfterPurchaseOrderApprovalAsync(
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