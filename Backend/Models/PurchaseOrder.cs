using System.Text.Json.Serialization;

namespace Backend.Models;

public enum PurchaseOrderStatus
{
    PendingApproval,
    Approved,
    Rejected
}

public enum DeliveryStatus
{
    NotSent,
    Ordered,
    Dispatched,
    InTransit,
    PartiallyDelivered,
    Delivered
}

public class PurchaseOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProcurementRequestId { get; set; }

    [JsonIgnore]
    public ProcurementRequest ProcurementRequest { get; set; } = default!;

    public Guid MaterialRequestId { get; set; }

    [JsonIgnore]
    public MaterialRequest MaterialRequest { get; set; } = default!;

    public string PoNumber { get; set; } = default!;

    public string MaterialCode { get; set; } = default!;
    public decimal Quantity { get; set; }

    public string SupplierId { get; set; } = default!;
    public string SupplierName { get; set; } = default!;

    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }

    public DateTime EstimatedDeliveryDate { get; set; }

    public PurchaseOrderStatus Status { get; set; }
        = PurchaseOrderStatus.PendingApproval;

    public string? DecidedBy { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DeliveryStatus DeliveryStatus { get; set; }
        = DeliveryStatus.NotSent;

    public DateTime? OrderedAt { get; set; }

    public decimal DeliveredQuantity { get; set; } = 0;

    public DateTime? ActualDeliveryDate { get; set; }

    public string SupplierTelegramChatId { get; set; } = default!;

    public VendorConfirmationStatus VendorConfirmationStatus { get; set; }
        = VendorConfirmationStatus.NotSent;

    public DateTime? SentForConfirmationAt { get; set; }

    public DateTime? VendorRespondedAt { get; set; }

    public decimal? VendorConfirmedQuantity { get; set; }
}

public enum VendorConfirmationStatus
{
    NotSent,
    Pending,
    Confirmed,
    Declined
}