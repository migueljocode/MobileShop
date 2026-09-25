namespace MobileShop.Models.ViewModels.Web;

/// <summary>
/// Data required to render a factor/report covering exactly the selected transactions.
/// </summary>
public sealed record TransactionFactorViewModel(
    IReadOnlyList<TransactionFactorRowViewModel> Rows,
    DateTime GeneratedAt)
{
    /// <summary>Sum of every included row's finished price.</summary>
    public decimal TotalPrice => Rows.Sum(row => row.FinishedPrice);
}