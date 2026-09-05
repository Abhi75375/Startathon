using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public class DeliveryTrackingController : ControllerBase
{
    private readonly DeliveryTrackingService _service;

    public DeliveryTrackingController(DeliveryTrackingService service)
    {
        _service = service;
    }

    [HttpPost("{id}/send-order")]
    public async Task<IActionResult> SendOrder(Guid id)
    {
        var po = await _service.SendOrderAsync(id);
        return Ok(po);
    }

    public record DeliveryUpdateDto(DeliveryStatus Status, decimal? DeliveredQuantity);

    [HttpPost("{id}/delivery-status")]
    public async Task<IActionResult> UpdateDeliveryStatus(Guid id, DeliveryUpdateDto dto)
    {
        var po = await _service.UpdateDeliveryStatusAsync(id, dto.Status, dto.DeliveredQuantity);
        return Ok(po);
    }
}