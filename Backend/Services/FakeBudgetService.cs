using Backend.Contracts;

namespace Backend.Services;

public class FakeBudgetService : IBudgetService
{
    public Task<BudgetInfo> GetMaxBudgetAsync(string materialCode)
    {
        // Hardcoded ceiling for testing - SUP-004 (25.00) will exceed this, others won't
        var budget = new BudgetInfo(materialCode, MaxPricePerUnit: 15.00m);
        return Task.FromResult(budget);
    }
}