namespace MobileShop.Services.PDF.Settings;

/// <summary>Configuration used by the invoice PDF generator.</summary>
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
