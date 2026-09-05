namespace Backend.Models;

public enum MaterialRequestStatus
{
    Created,
    InventoryChecked,
    Fulfilled,
    ShortageIdentified,
    SupplierSelected,
    NoSupplierAvailable,
    ProcurementRequested,
    ProcurementApproved,
    ProcurementRejected,
    PoPendingApproval,
    PoApproved,
    PoRejected,
    AwaitingVendorConfirmation,
    Ordered,
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

    public Guid? ProjectId { get; set; }
    public decimal ShortageQuantity { get; set; }

    public string? SelectedSupplierId { get; set; }
    public string? SelectedSupplierName { get; set; }
    public decimal? SelectedSupplierPrice { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }

    public string? SelectedSupplierTelegramChatId { get; set; }
    public string? ExcludedSupplierIds { get; set;}
}