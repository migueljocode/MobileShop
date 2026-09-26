using MobileShop.Models.ViewModels;

namespace MobileShop.Tests.Models.ViewModels;

/// <summary>
/// Verifies the computed profit percentage on <see cref="ProfitLossRowViewModel"/>:
/// - Positive profit with non-zero Bought returns (Profit / Bought) * 100.
/// - Zero profit returns 0%.
/// - Negative profit returns negative percentage.
/// - Zero Bought returns 0% (no division by zero).
/// </summary>
public class ProfitLossRowViewModelTests
{
        [Fact]
    public void Positive_profit_returns_correct_percentage()
    {
        var row = new ProfitLossRowViewModel(1, "Product A", 100m, 150m);

        Assert.Equal(50m, row.Profit);
        Assert.Equal(50m, row.ProfitPercent); // (50 / 100) * 100 = 50%
    }

    [Fact]
    public void Zero_profit_returns_zero_percent()
    {
        var row = new ProfitLossRowViewModel(1, "Product A", 100m, 100m);

        Assert.Equal(0m, row.Profit);
        Assert.Equal(0m, row.ProfitPercent);
    }

    [Fact]
    public void Negative_profit_returns_negative_percentage()
    {
        var row = new ProfitLossRowViewModel(1, "Product A", 100m, 50m);

        Assert.Equal(-50m, row.Profit);
        Assert.Equal(-50m, row.ProfitPercent); // (-50 / 100) * 100 = -50%
    }

    [Fact]
    public void Zero_bought_returns_zero_percent_without_division_by_zero()
    {
        var row = new ProfitLossRowViewModel(1, "Product A", 0m, 50m);

        Assert.Equal(50m, row.Profit);
        Assert.Equal(0m, row.ProfitPercent); // Bought == 0 → safe zero
    }
}