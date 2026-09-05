namespace Backend.Contracts;

public interface ISupplierService
{
    Task<List<SupplierOffer>> GetSuppliersAsync(string materialCode);
}

public record SupplierOffer(
    string SupplierId,
    string SupplierName,
    string MaterialCode,
    decimal PricePerUnit,
    DateTime DeliveryDate,
    decimal ReliabilityScore, // 0.0 to 1.0
    decimal Rating,            // 0 to 5
    string TelegramChatId
);