using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/material-estimates")]
public class MaterialEstimationController : ControllerBase
{
    private readonly MaterialEstimationService _service;

    public MaterialEstimationController(MaterialEstimationService service)
    {
        _service = service;
    }

    [HttpGet("{projectId}")]
    public async Task<IActionResult> Estimate(Guid projectId)
    {
        var estimates = await _service.EstimateAsync(projectId);
        return Ok(estimates);
    }
}