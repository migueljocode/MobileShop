namespace MobileShop.Models.ViewModels.Web;

public sealed record ProductTransactionViewModel(
    DateTime Date,
    TransactionDirection Direction,
    decimal FinishedPrice,
    string SellerLabel,
    string CustomerLabel);
