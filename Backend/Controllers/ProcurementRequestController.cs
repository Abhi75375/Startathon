using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/material-requests")]
public class ProcurementRequestController : ControllerBase
{
    private readonly ProcurementRequestService _service;

    public ProcurementRequestController(ProcurementRequestService service)
    {
        _service = service;
    }

    [HttpPost("{id}/generate-procurement-request")]
    public async Task<IActionResult> Generate(Guid id)
    {
        var procurementRequest = await _service.GenerateAsync(id);
        return Ok(procurementRequest);
    }
}