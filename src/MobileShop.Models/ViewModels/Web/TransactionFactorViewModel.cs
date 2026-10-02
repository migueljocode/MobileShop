namespace MobileShop.Models.ViewModels.Web;

/// <summary>
/// Data required to render a customer-facing factor/report covering exactly the selected transactions.
/// Static shop identity and contact information are supplied by PDF configuration rather than duplicated here.
/// </summary>
public sealed record TransactionFactorViewModel(
    IReadOnlyList<TransactionFactorRowViewModel> Rows,
    DateTime GeneratedAt)
{
    /// <summary>Sum of every included row's finished price.</summary>
    public decimal TotalPrice => Rows.Sum(row => row.FinishedPrice);
}
