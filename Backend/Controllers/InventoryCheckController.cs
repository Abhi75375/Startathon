using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/material-requests")]
public class InventoryCheckController : ControllerBase
{
    private readonly InventoryCheckService _service;

    public InventoryCheckController(InventoryCheckService service)
    {
        _service = service;
    }

    [HttpPost("{id}/check-inventory")]
    public async Task<IActionResult> Check(Guid id)
    {
        var result = await _service.CheckAsync(id);
        return Ok(result);
    }
}