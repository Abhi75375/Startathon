namespace Backend.Contracts;

public record MaterialEstimationApprovalPayload(
    Guid ReviewId,
    Guid ProjectId,
    List<MaterialEstimationApprovalItem> Materials);

public record MaterialEstimationApprovalItem(
    string MaterialCode,
    decimal EstimatedQuantity);