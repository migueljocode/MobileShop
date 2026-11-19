namespace MobileShop.Services.PDF.Settings;

/// <summary>
/// Configuration used by the invoice PDF generator.
/// The <c>Shop*</c> properties serve as first-run seed defaults; live shop values come from the
/// database (<see cref="ShopProfile"/>) via <see cref="IShopProfileDataService"/>.
/// </summary>
public sealed class PdfSettings
{
    public string PageSize { get; set; } = "A4";
    public float MarginTop { get; set; } = 36;
    public float MarginRight { get; set; } = 36;
    public float MarginBottom { get; set; } = 36;
    public float MarginLeft { get; set; } = 36;
    public string ShopName { get; set; } = "Mobile Shop";
    public string ShopAddress { get; set; } = string.Empty;
    public string ShopPhone { get; set; } = string.Empty;
    public string ShopInstagram { get; set; } = string.Empty;
}
