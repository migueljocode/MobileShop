namespace MobileShop.Models.ViewModels.Web;

public sealed record ProductDetailsViewModel(
    string Type,
    int ProductId,
    string Manufacturer,
    string Model,
    string Identifier,
    string? Color,
    string OwnerLabel,
    string GuaranteeLabel,
    bool IsSecondHand)
{
    /// <summary>
    /// The product's transaction history. Defaults to an empty list so callers that never populate it
    /// cannot dereference <see langword="null"/>.
    /// </summary>
    public IReadOnlyList<ProductTransactionViewModel> Transactions { get; init; } = [];
}
