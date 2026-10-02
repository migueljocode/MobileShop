namespace MobileShop.Models.ViewModels.Web;

public sealed record ProductTransactionViewModel(
    DateTime Date,
    TransactionDirection Direction,
    int FinishedPrice,
    string SellerLabel,
    string CustomerLabel);
