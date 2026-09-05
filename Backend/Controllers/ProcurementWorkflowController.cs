using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/workflow")]
public class ProcurementWorkflowController : ControllerBase
{
    private readonly IProcurementWorkflowService _workflowService;

    public ProcurementWorkflowController(
        IProcurementWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpPost("start/{materialRequestId:guid}")]
    public async Task<IActionResult> Start(
        Guid materialRequestId)
    {
        try
        {
            await _workflowService
                .StartFromMaterialRequestAsync(
                    materialRequestId);

            return Ok(new
            {
                message = "Procurement workflow started.",
                materialRequestId
            });
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