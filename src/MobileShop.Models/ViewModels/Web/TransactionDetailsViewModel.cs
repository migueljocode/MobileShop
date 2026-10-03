namespace MobileShop.Models.ViewModels.Web;

public sealed record TransactionDetailsViewModel(
    DateTime Date,
    TransactionDirection Direction,
    long FinishedPrice,
    string ProductLabel,
    string SellerLabel,
    string CustomerLabel);
