using Backend.Contracts;
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

    [HttpPost]
    public async Task<IActionResult> Estimate(ProjectData project)
    {
        var estimates = await _service.EstimateAsync(project);

        return Ok(estimates);
    }
}