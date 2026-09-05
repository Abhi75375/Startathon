using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api")]
public class ProcurementRequestController : ControllerBase
{
    private readonly ProcurementRequestService _service;

    public ProcurementRequestController(ProcurementRequestService service)
    {
        _service = service;
    }

    [HttpPost("material-requests/{id}/generate-procurement-request")]
    public async Task<IActionResult> Generate(Guid id)
    {
        var procurementRequest = await _service.GenerateAsync(id);
        return Ok(procurementRequest);
    }

    public record ProcurementDecisionDto(string DecidedBy, bool Approved, string? RejectionReason);

    [HttpPost("procurement-requests/{id}/decision")]
    public async Task<IActionResult> RecordDecision(Guid id, ProcurementDecisionDto dto)
    {
        var result = await _service.RecordDecisionAsync(id, dto.DecidedBy, dto.Approved, dto.RejectionReason);
        return Ok(result);
    }
}