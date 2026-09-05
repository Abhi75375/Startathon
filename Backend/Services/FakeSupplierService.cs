using Backend.Contracts;

namespace Backend.Services;

public class FakeSupplierService : ISupplierService
{
    public Task<List<SupplierOffer>> GetSuppliersAsync(string materialCode)
    {
        var offers = new List<SupplierOffer>
        {
            new("SUP-001", "ABC Traders", materialCode, PricePerUnit: 12.50m,
                DeliveryDate: DateTime.UtcNow.AddDays(15), ReliabilityScore: 0.92m, Rating: 4.5m, TelegramChatId: "111111111"),

            new("SUP-002", "FastBuild Supplies", materialCode, PricePerUnit: 14.00m,
                DeliveryDate: DateTime.UtcNow.AddDays(10), ReliabilityScore: 0.85m, Rating: 4.2m, TelegramChatId: "222222222"),

            new("SUP-003", "CheapCo", materialCode, PricePerUnit: 9.00m,
                DeliveryDate: DateTime.UtcNow.AddDays(45), ReliabilityScore: 0.70m, Rating: 3.5m, TelegramChatId: "333333333"),

            new("SUP-004", "Premium Materials Inc", materialCode, PricePerUnit: 25.00m,
                DeliveryDate: DateTime.UtcNow.AddDays(5), ReliabilityScore: 0.98m, Rating: 4.9m, TelegramChatId: "444444444")
        };

        return Task.FromResult(offers);
    }
}