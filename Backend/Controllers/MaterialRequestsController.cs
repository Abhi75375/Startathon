using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/material-requests")]
public class MaterialRequestsController : ControllerBase
{
    private readonly ProcurementDbContext _db;

    public MaterialRequestsController(ProcurementDbContext db)
    {
        _db = db;
    }

    public record CreateMaterialRequestDto(string MaterialCode, decimal Quantity, string RequestedBy, bool GeneratedByAi);

    [HttpPost]
    public async Task<IActionResult> Create(CreateMaterialRequestDto dto)
    {
        var request = new MaterialRequest
        {
            MaterialCode = dto.MaterialCode,
            QuantityRequested = dto.Quantity,
            RequestedBy = dto.RequestedBy,
            GeneratedByAi = dto.GeneratedByAi
        };

        _db.MaterialRequests.Add(request);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var request = await _db.MaterialRequests.FindAsync(id);
        return request is null ? NotFound() : Ok(request);
    }
}