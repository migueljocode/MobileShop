namespace MobileShop.Models.ViewModels.Web;

public sealed record ProductTransactionViewModel(
    DateTime Date,
    TransactionDirection Direction,
    long FinishedPrice,
    string SellerLabel,
    string CustomerLabel);
