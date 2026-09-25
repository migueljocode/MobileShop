namespace MobileShop.Models.ViewModels.Web;

/// <summary>
/// One transaction line of a generated factor/report. <see cref="PersonRole"/> tells which party is
/// relevant for the row - the customer on a sale, the seller on a purchase.
/// </summary>
public sealed record TransactionFactorRowViewModel(
    int TransactionId,
    DateTime Date,
    TransactionDirection Direction,
    string ProductLabel,
    decimal FinishedPrice,
    string PersonRole,
    string PersonLabel);