namespace MobileShop.Models.ViewModels.Web;

public sealed record TransactionListItemViewModel(
    int Id,
    DateTime Date,
    TransactionDirection Direction,
    string ProductLabel,
    long FinishedPrice,
    string SellerLabel,
    string CustomerLabel)
{
    public int CustomerId { get; init; }
}
