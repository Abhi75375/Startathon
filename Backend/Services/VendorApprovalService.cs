using Backend.Contracts;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class VendorApprovalService
{
    private readonly ProcurementDbContext _db;
    private readonly ISupplierService _supplierService;
    private readonly IVendorApprovalGateway _gateway;

    public VendorApprovalService(
        ProcurementDbContext db,
        ISupplierService supplierService,
        IVendorApprovalGateway gateway)
    {
        _db = db;
        _supplierService = supplierService;
        _gateway = gateway;
    }

    public async Task<VendorApprovalPayload> CreateAndSubmitAsync(
        Guid materialRequestId)
    {
        var request = await _db.MaterialRequests
            .FirstOrDefaultAsync(x => x.Id == materialRequestId)
            ?? throw new InvalidOperationException(
                "Material request not found.");

        var offers =
            await _supplierService.GetSuppliersAsync(
                request.MaterialCode);

        var excludedIds =
            (request.ExcludedSupplierIds ?? "")
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .ToHashSet();

        var eligibleOffers = offers
            .Where(x => !excludedIds.Contains(x.SupplierId))
            .OrderBy(x => x.PricePerUnit)
            .ToList();

        if (eligibleOffers.Count == 0)
        {
            request.Status =
                MaterialRequestStatus.NoSupplierAvailable;

            await _db.SaveChangesAsync();

            throw new InvalidOperationException(
                "No eligible vendors found.");
        }

        var quantity =
            request.ShortageQuantity > 0
                ? request.ShortageQuantity
                : request.QuantityRequested;

        var items = eligibleOffers
            .Select(x => new VendorApprovalItem(
                MaterialCode: request.MaterialCode,
                Quantity: quantity,
                SupplierId: x.SupplierId,
                SupplierName: x.SupplierName,
                UnitPrice: x.PricePerUnit,
                TotalAmount: quantity * x.PricePerUnit,
                EstimatedDeliveryDate: x.DeliveryDate))
            .ToList();

        var payload = new VendorApprovalPayload(
            MaterialRequestId: request.Id,
            ProjectId: request.ProjectId,
            Items: items);

        await _gateway.SubmitForApprovalAsync(payload);

        return payload;
    }

    public async Task<MaterialRequest> RecordApprovalAsync(
        VendorApprovalDecision decision,
        string approvedBy)
    {
        var request = await _db.MaterialRequests
            .FirstOrDefaultAsync(
                x => x.Id == decision.MaterialRequestId)
            ?? throw new InvalidOperationException(
                "Material request not found.");

        if (decision.ProjectId.HasValue &&
            request.ProjectId != decision.ProjectId.Value)
        {
            throw new InvalidOperationException(
                "ProjectId does not match the material request.");
        }

        if (decision.Items is null ||
            decision.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "No approved vendor data supplied.");
        }

        var selectedVendor = decision.Items.First();

        var availableVendors =
            await _supplierService.GetSuppliersAsync(
                request.MaterialCode);

        var selectedOffer =
            availableVendors.FirstOrDefault(
                x => x.SupplierId == selectedVendor.SupplierId);

        if (selectedOffer is null)
        {
            throw new InvalidOperationException(
                "Selected supplier does not exist for this material.");
        }

        var excludedIds =
            (request.ExcludedSupplierIds ?? "")
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .ToHashSet();

        if (excludedIds.Contains(selectedOffer.SupplierId))
        {
            throw new InvalidOperationException(
                "Selected supplier is excluded for this material request.");
        }

        var quantity =
            request.ShortageQuantity > 0
                ? request.ShortageQuantity
                : request.QuantityRequested;

        var approvedQuantity =
            selectedVendor.Quantity > 0
                ? selectedVendor.Quantity
                : quantity;

        if (approvedQuantity > quantity)
        {
            approvedQuantity = quantity;
        }

        request.SelectedSupplierId =
            selectedOffer.SupplierId;

        request.SelectedSupplierName =
            selectedOffer.SupplierName;

        request.SelectedSupplierPrice =
            selectedOffer.PricePerUnit;

        request.SelectedSupplierTelegramChatId =
            selectedOffer.TelegramChatId;

        request.EstimatedDeliveryDate =
            selectedOffer.DeliveryDate;

        request.Status =
            MaterialRequestStatus.SupplierSelected;

        await _db.SaveChangesAsync();

        return request;
    }
}