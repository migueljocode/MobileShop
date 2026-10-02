namespace MobileShop.Models.ViewModels.Web;

public sealed record TransactionListItemViewModel(
    int Id,
    DateTime Date,
    TransactionDirection Direction,
    string ProductLabel,
    int FinishedPrice,
    string SellerLabel,
    string CustomerLabel);
