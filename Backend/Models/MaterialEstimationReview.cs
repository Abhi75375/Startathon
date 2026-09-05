namespace Backend.Models;

public enum ReviewStatus
{
    PendingReview,
    Approved,
    Rejected
}

public class MaterialEstimationReview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.PendingReview;
    public string? ReviewedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public List<MaterialEstimationReviewItem> Items { get; set; } = new();
    public List<MaterialRequest> MaterialRequests { get; set; } = new();
}

public class MaterialEstimationReviewItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MaterialEstimationReviewId { get; set; }
    public string MaterialCode { get; set; } = default!;
    public decimal AiEstimatedQuantity { get; set; }   // what the algorithm originally said
    public decimal? FinalQuantity { get; set; }         // what the supervisor approved/edited to
    public bool Approved { get; set; }                  // did the supervisor keep this material in the request at all
}