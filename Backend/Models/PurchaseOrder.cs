namespace Backend.Models;

public enum PurchaseOrderStatus { PendingApproval, Approved, Rejected }

public enum DeliveryStatus 
{
    NotSent, Ordered, Dispatched, InTransit, PartiallyDelivered, Delivered
}

public class PurchaseOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProcurementRequestId { get; set; }
    public Guid MaterialRequestId { get; set; }
    public string PoNumber { get; set; } = default!;
    public string MaterialCode { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string SupplierId { get; set; } = default!;
    public string SupplierName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.PendingApproval;
    public string? DecidedBy { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // delivery tracking
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.NotSent;
    public DateTime? OrderedAt { get; set; }
    public decimal DeliveredQuantity { get; set; } = 0;
    public DateTime? ActualDeliveryDate { get; set; } // set when DeliveryStatus reaches Delivered
    public string SupplierTelegramChatId { get; set; } = default!;
    public VendorConfirmationStatus VendorConfirmationStatus { get; set; } = VendorConfirmationStatus.NotSent; // NEW
public DateTime? SentForConfirmationAt { get; set; } // NEW
public DateTime? VendorRespondedAt { get; set; } // NEW
public decimal? VendorConfirmedQuantity { get; set; } // NEW
}


public enum VendorConfirmationStatus  // NEW
{
    NotSent,
    Pending,
    Confirmed,
    Declined
}
