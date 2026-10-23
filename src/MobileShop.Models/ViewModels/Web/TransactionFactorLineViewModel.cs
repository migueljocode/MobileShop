namespace MobileShop.Models.ViewModels.Web;

/// <summary>
/// One printed line of a factor: a group of factor rows that share the same product specs, unit price
/// and party. <see cref="Quantity"/> is how many units the line covers.
/// </summary>
public sealed record TransactionFactorLineViewModel(
    string ProductText,
    int Quantity,
    long UnitPrice,
    string PersonLabel)
{
    /// <summary>Quantity multiplied by the unit price (<see cref="UnitPrice"/>).</summary>
    public long PaidAmount => UnitPrice * Quantity;
}
