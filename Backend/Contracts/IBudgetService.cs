namespace Backend.Contracts;

public interface IBudgetService
{
    Task<BudgetInfo> GetMaxBudgetAsync(string materialCode);
}

public record BudgetInfo(string MaterialCode, decimal MaxPricePerUnit);