using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/material-requests")]
public class SupplierSelectionController : ControllerBase
{
    private readonly SupplierSelectionService _service;

    public SupplierSelectionController(SupplierSelectionService service)
    {
        _service = service;
    }

    [HttpPost("{id}/select-supplier")]
    public async Task<IActionResult> SelectSupplier(Guid id)
    {
        var result = await _service.SelectSupplierAsync(id);
        return Ok(result);
    }
}
