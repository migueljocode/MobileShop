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
}
