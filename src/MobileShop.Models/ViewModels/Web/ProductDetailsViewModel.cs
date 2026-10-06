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

    /// <summary>
    /// The manufacturer part number code, or <c>N/A</c> when the phone carries no part number
    /// (or the product is not a phone).
    /// </summary>
    public string PartNumberLabel { get; init; } = "N/A";

    /// <summary>
    /// Whether the part number supports dual SIM, rendered as <c>Yes</c>/<c>No</c>. Stays <c>N/A</c>
    /// when there is no part number, so a missing part number is never shown as <c>No</c>.
    /// </summary>
    public string DualSimLabel { get; init; } = "N/A";

    /// <summary>
    /// Whether the part number supports eSIM, rendered as <c>Yes</c>/<c>No</c>. Stays <c>N/A</c>
    /// when there is no part number, so a missing part number is never shown as <c>No</c>.
    /// </summary>
    public string EsimLabel { get; init; } = "N/A";

    public string? Cpu { get; init; }
    public string? Gpu { get; init; }
    public decimal? DisplaySize { get; init; }
    public string? Notes { get; init; }
}
