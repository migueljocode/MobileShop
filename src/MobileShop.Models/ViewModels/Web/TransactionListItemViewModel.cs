namespace MobileShop.Models.ViewModels.Web;

public sealed record TransactionListItemViewModel(
    int Id,
    DateTime Date,
    TransactionDirection Direction,
    string ProductLabel,
    long FinishedPrice,
    string SellerLabel,
    string CustomerLabel)
{
    public int CustomerId { get; init; }

    /// <summary>Gets whether this row is the sale of an Apple ID, which has its own delivery factor.</summary>
    public bool HasAppleIdFactor { get; init; }
}
