namespace MobileShop.Models.ViewModels.Web;

public sealed record TransactionListItemViewModel(
    int Id,
    DateTime Date,
    TransactionDirection Direction,
    string ProductLabel,
    decimal FinishedPrice,
    string SellerLabel,
    string CustomerLabel);
