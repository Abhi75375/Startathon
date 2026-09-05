namespace Backend.Models;

public enum MaterialRequestStatus
{
    Created,
    InventoryChecked,
    Fulfilled,          // enough stock existed, no procurement needed
    ShortageIdentified, // triggers procurement flow
    Cancelled
}

public class MaterialRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MaterialCode { get; set; } = default!;   // links to ERP material code
    public decimal QuantityRequested { get; set; }
    public string RequestedBy { get; set; } = default!;    // supervisor name/id
    public bool GeneratedByAi { get; set; }
    public MaterialRequestStatus Status { get; set; } = MaterialRequestStatus.Created;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}