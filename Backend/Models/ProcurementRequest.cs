namespace Backend.Models;

public enum ProcurementRequestStatus
{
    PendingApproval,
    Approved,
    Rejected
}

public class ProcurementRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MaterialRequestId { get; set; }

    public string MaterialCode { get; set; } = default!;
    public decimal Quantity { get; set; }

    public string SupplierId { get; set; } = default!;
    public string SupplierName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }

    public ProcurementRequestStatus Status { get; set; } = ProcurementRequestStatus.PendingApproval;

    public string? DecidedBy { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string SupplierTelegramChatId { get; set; } = default!;
}