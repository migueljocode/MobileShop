namespace MobileShop.Models.ViewModels.Web;

public sealed record ProfitLossRowViewModel(
    int ProductId,
    string ProductLabel,
    long Bought,
    long Sold)
{
    public long Profit => Sold - Bought;

    /// <summary>
    /// Profit as a percentage of the amount bought: (Profit / Bought) * 100.
    /// Returns 0 when Bought is zero to avoid division by zero.
    /// </summary>
    public decimal ProfitPercent => Bought != 0
        ? (Profit / (decimal)Bought) * 100m
        : 0m;
}
