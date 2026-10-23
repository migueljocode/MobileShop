namespace MobileShop.Models.ViewModels.Web;

/// <summary>
/// Data required to render a customer-facing factor/report covering exactly the selected transactions.
/// Static shop identity and contact information are supplied by PDF configuration rather than duplicated here.
/// </summary>
public sealed record TransactionFactorViewModel(
    IReadOnlyList<TransactionFactorRowViewModel> Rows,
    DateTime GeneratedAt)
{
    private const string LabelSeparator = " · ";

    /// <summary>Sum of every included row's finished price.</summary>
    public long TotalPrice => Rows.Sum(row => row.FinishedPrice);

    /// <summary>
    /// Rows grouped into printed lines. Rows merge only when their specs, unit price, direction and party
    /// all match; lines keep the order in which their first row appears.
    /// </summary>
    public IReadOnlyList<TransactionFactorLineViewModel> Lines =>
        Rows.GroupBy(row => (row.ProductLabel, row.SpecKey, row.Direction, row.FinishedPrice, row.PersonLabel))
            .Select(group => new TransactionFactorLineViewModel(
                ProductTextOf(group.Key.ProductLabel, group.First().SpecLabel),
                group.Count(),
                group.Key.FinishedPrice,
                group.Key.PersonLabel))
            .ToList();

    private static string ProductTextOf(string productLabel, string specLabel)
        => string.IsNullOrEmpty(specLabel) ? productLabel : productLabel + LabelSeparator + specLabel;
}
