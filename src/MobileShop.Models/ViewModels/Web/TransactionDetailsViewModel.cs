namespace MobileShop.Models.ViewModels.Web;

public sealed record TransactionDetailsViewModel(
    DateTime Date,
    TransactionDirection Direction,
    int FinishedPrice,
    string ProductLabel,
    string SellerLabel,
    string CustomerLabel);
