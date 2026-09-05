namespace Backend.Models;

public class MaterialEstimate
{
    public string MaterialCode { get; set; } = default!;
    public decimal EstimatedQuantity { get; set; }
    public int SampleSize { get; set; }          // how many historical projects informed this
    public decimal ConfidenceScore { get; set; } // 0.0 (unreliable) to 1.0 (very consistent)
    public bool NeedsLlmReview { get; set; }      // true if algorithm isn't confident enough
}