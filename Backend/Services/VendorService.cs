using Backend.Contracts;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class VendorService : ISupplierService
{
    private readonly ProcurementDbContext _db;

    public VendorService(ProcurementDbContext db)
    {
        _db = db;
    }

    public async Task<List<SupplierOffer>> GetSuppliersAsync(string materialCode)
    {
        var vendors = await _db.Vendors
            .Where(v => v.MaterialCode == materialCode)
            .ToListAsync();

        return vendors.Select(v => new SupplierOffer(
            v.Id,
            v.Name,
            materialCode,
            v.PricePerUnit,
            DateTime.UtcNow.AddDays(v.DeliveryDays),
            v.ReliabilityScore,
            v.Rating,
            v.ChatId ?? ""
        )).ToList();
    }
}