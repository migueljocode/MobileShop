namespace MobileShop.Models.ViewModels.Web;

public sealed record ProductListItemViewModel(
    int EntityId,
    int ProductId,
    string Type,
    string Name,
    string Identifier,
    string? Color,
    bool IsSold,
    bool IsSecondHand)
{
    /// <summary>
    /// The manufacturer part number code for the row, or <c>N/A</c> when the row carries no part
    /// number (Apple IDs and phones without one). Kept out of the positional parameters so existing
    /// constructions stay source-compatible.
    /// </summary>
    public string PartNumberLabel { get; init; } = "N/A";

    /// <summary>
    /// The catalog (inventory) price of the underlying product, used as the suggested finished
    /// price on the transaction forms. Null when the projection did not load a product price.
    /// Kept out of the positional parameters so existing constructions stay source-compatible.
    /// </summary>
    public long? SuggestedPrice { get; init; }

    public int? ManufacturerId { get; init; }
    public string? ManufacturerName { get; init; }
    public int? ModelId { get; init; }
    public string? ModelName { get; init; }
}
