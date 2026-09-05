using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api")]
public class PurchaseOrderController : ControllerBase
{
    private readonly PurchaseOrderService _service;

    public PurchaseOrderController(PurchaseOrderService service)
    {
        _service = service;
    }

    [HttpPost("procurement-requests/{procurementRequestId}/generate-po")]
    public async Task<IActionResult> Generate(Guid procurementRequestId)
    {
        var po = await _service.GenerateAsync(procurementRequestId);
        return Ok(po);
    }

    public record PoDecisionDto(string DecidedBy, bool Approved, string? RejectionReason);

    [HttpPost("purchase-orders/{id}/decision")]
    public async Task<IActionResult> RecordDecision(Guid id, PoDecisionDto dto)
    {
        var result = await _service.RecordDecisionAsync(id, dto.DecidedBy, dto.Approved, dto.RejectionReason);
        return Ok(result);
    }
}