namespace MobileShop.Models.ViewModels.Web;

public sealed record ProfitLossRowViewModel(
    int ProductId,
    string ProductLabel,
    decimal Bought,
    decimal Sold)
{
    public decimal Profit => Sold - Bought;

    /// <summary>
    /// Profit as a percentage of the amount bought: (Profit / Bought) * 100.
    /// Returns 0 when Bought is zero to avoid division by zero.
    /// </summary>
    public decimal ProfitPercent => Bought != 0
        ? (Profit / Bought) * 100m
        : 0m;
}
