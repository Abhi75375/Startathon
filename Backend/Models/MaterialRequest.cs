namespace Backend.Models;

public enum MaterialRequestStatus
{
    Created,
    InventoryChecked,
    Fulfilled,
    ShortageIdentified,
    SupplierSelected,      // NEW
    NoSupplierAvailable,   // NEW - every supplier failed deadline/budget filters
    Cancelled
}

public class MaterialRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MaterialCode { get; set; } = default!;
    public decimal QuantityRequested { get; set; }
    public string RequestedBy { get; set; } = default!;
    public bool GeneratedByAi { get; set; }
    public MaterialRequestStatus Status { get; set; } = MaterialRequestStatus.Created;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // NEW fields below
    public Guid? ProjectId { get; set; }                    // null for manual requests without a linked project
    public decimal ShortageQuantity { get; set; }            // persisted from Step 2/3, not just calculated in-memory

    public string? SelectedSupplierId { get; set; }
    public string? SelectedSupplierName { get; set; }
    public decimal? SelectedSupplierPrice { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}