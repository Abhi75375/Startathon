namespace Backend.Models;

public class SupplierPerformanceRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PurchaseOrderId { get; set; }
    public string SupplierId { get; set; } = default!;
    public string SupplierName { get; set; } = default!;
    public string MaterialCode { get; set; } = default!;

    public DateTime EstimatedDeliveryDate { get; set; }
    public DateTime ActualDeliveryDate { get; set; }
    public bool OnTime { get; set; }
    public int DaysLate { get; set; } // 0 if on time

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}