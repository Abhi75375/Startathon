using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api")]
public class DeliveryTrackingController : ControllerBase
{
    private readonly DeliveryTrackingService _service;
    private readonly ProcurementDbContext _db;

    public DeliveryTrackingController(
        DeliveryTrackingService service,
        ProcurementDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpPost("purchase-orders/{id}/send-order")]
    public async Task<IActionResult> SendOrder(Guid id)
    {
        try
        {
            var po =
                await _service.SendOrderAsync(id);

            return Ok(po);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpPost("purchase-orders/{id}/delivery-status")]
    public async Task<IActionResult> UpdateDeliveryStatus(
        Guid id,
        [FromBody] DeliveryUpdateDto dto)
    {
        try
        {
            var po =
                await _service.UpdateDeliveryStatusAsync(
                    id,
                    dto.Status,
                    dto.DeliveredQuantity);

            return Ok(po);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }

    [HttpGet("projects/{projectId}/procurement-tracking")]
    public async Task<IActionResult> GetProjectTracking(
        Guid projectId)
    {
        var materialRequests =
            await _db.MaterialRequests
                .AsNoTracking()
                .Where(x => x.ProjectId == projectId)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new
                {
                    materialRequestId = x.Id,
                    materialCode = x.MaterialCode,
                    quantityRequested = x.QuantityRequested,
                    shortageQuantity = x.ShortageQuantity,
                    status = x.Status,
                    createdAt = x.CreatedAt,

                    selectedSupplier = new
                    {
                        supplierId = x.SelectedSupplierId,
                        supplierName = x.SelectedSupplierName,
                        unitPrice = x.SelectedSupplierPrice,
                        estimatedDeliveryDate =
                            x.EstimatedDeliveryDate
                    },

                    purchaseOrders = x.PurchaseOrders
                        .OrderBy(po => po.CreatedAt)
                        .Select(po => new
                        {
                            purchaseOrderId = po.Id,
                            poNumber = po.PoNumber,

                            materialCode = po.MaterialCode,
                            quantity = po.Quantity,

                            supplierId = po.SupplierId,
                            supplierName = po.SupplierName,

                            unitPrice = po.UnitPrice,
                            totalCost = po.TotalCost,

                            estimatedDeliveryDate =
                                po.EstimatedDeliveryDate,

                            status = po.Status,

                            deliveryStatus =
                                po.DeliveryStatus,

                            vendorConfirmationStatus =
                                po.VendorConfirmationStatus,

                            vendorConfirmedQuantity =
                                po.VendorConfirmedQuantity,

                            deliveredQuantity =
                                po.DeliveredQuantity,

                            orderedAt =
                                po.OrderedAt,

                            sentForConfirmationAt =
                                po.SentForConfirmationAt,

                            vendorRespondedAt =
                                po.VendorRespondedAt,

                            actualDeliveryDate =
                                po.ActualDeliveryDate,

                            createdAt =
                                po.CreatedAt
                        })
                        .ToList()
                })
                .ToListAsync();

        return Ok(new
        {
            projectId,
            generatedAt = DateTime.UtcNow,
            materialRequests
        });
    }

    public record DeliveryUpdateDto(
        DeliveryStatus Status,
        decimal? DeliveredQuantity);
}