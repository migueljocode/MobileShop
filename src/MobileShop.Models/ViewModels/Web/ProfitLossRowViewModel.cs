namespace MobileShop.Models.ViewModels.Web;

public sealed record ProfitLossRowViewModel(
    int ProductId,
    string ProductLabel,
    decimal Bought,
    decimal Sold)
{
    public decimal Profit => Sold - Bought;
}
