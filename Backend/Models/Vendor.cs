namespace Backend.Models;

public class Vendor
{
    public string Id { get; set; } = default!;       // e.g. "SUP-001"
    public string Name { get; set; } = default!;
    public string MaterialCode { get; set; } = default!;
    public decimal PricePerUnit { get; set; }
    public int DeliveryDays { get; set; }             // relative days, not a fixed date - computed at selection time
    public decimal ReliabilityScore { get; set; }      // 0.0 to 1.0
    public decimal Rating { get; set; }                 // 0 to 5
    public string? ChatId { get; set; }                 // Telegram chat ID - you'll fill this in manually
}