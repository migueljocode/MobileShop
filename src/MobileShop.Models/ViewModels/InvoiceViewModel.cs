namespace MobileShop.Models.ViewModels;

/// <summary>
/// Minimal invoice data contract used by the PDF generator.
/// </summary>
public sealed record InvoiceViewModel(
    string Title = "Invoice",
    string? ShopName = null,
    string? CustomerName = null,
    string? Details = null);
