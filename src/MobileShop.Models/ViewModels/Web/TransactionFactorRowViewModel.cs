namespace MobileShop.Models.ViewModels.Web;

/// <summary>
/// One transaction line of a generated factor/report. <see cref="PersonRole"/> tells which party is
/// relevant for the row - the customer on a sale, the seller on a purchase. <see cref="SpecKey"/> and
/// <see cref="SpecLabel"/> identify the product's specs, so identical units can be grouped on a factor.
/// </summary>
public sealed record TransactionFactorRowViewModel(
    int TransactionId,
    DateTime Date,
    TransactionDirection Direction,
    string ProductLabel,
    long FinishedPrice,
    string PersonRole,
    string PersonLabel,
    string SpecKey = "",
    string SpecLabel = "");
