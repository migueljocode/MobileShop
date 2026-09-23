namespace MobileShop.Services.PDF.Configuration;

/// <summary>QuestPDF-backed invoice generator.</summary>
/// <remarks>This is the only class in the solution that references QuestPDF types.</remarks>
public sealed class QuestPdfGenerator(IOptions<PdfSettings> options) : IPdfGenerator
{
    private readonly PdfSettings _settings = options.Value;

    /// <inheritdoc />
    public byte[] Generate(InvoiceViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var presentation = Resolve(model);
        return Create(renderDocument).GeneratePdf();

        void renderDocument(IDocumentContainer container) => container.Page(renderPage);
        void renderPage(PageDescriptor page) => ComposePage(page, presentation);
    }

    private void ComposePage(PageDescriptor page, InvoicePresentation p)
    {
        page.Size(ParsePageSize(_settings.PageSize));
        page.MarginTop(_settings.MarginTop);
        page.MarginRight(_settings.MarginRight);
        page.MarginBottom(_settings.MarginBottom);
        page.MarginLeft(_settings.MarginLeft);

        page.Header().Column(c => RenderHeader(c, p));
        page.Content().Column(c => RenderContent(c, p));
        page.Footer().AlignCenter().Text(t => RenderFooter(t));
    }
private static void RenderHeader(ColumnDescriptor column, InvoicePresentation p)
    {
        column.Item().Text(p.ShopName).Bold().FontSize(18);
        column.Item().Text(p.InvoiceKind).FontSize(14);
    }

    private void RenderContent(ColumnDescriptor column, InvoicePresentation p)
    {
        RenderPartyInfo(column, p);
        RenderProductInfo(column, p);
        RenderGuaranteeInfo(column, p);
        RenderNotesAndSignature(column, p);
    }

    private static void RenderPartyInfo(ColumnDescriptor column, InvoicePresentation p)
    {
        if (!string.IsNullOrWhiteSpace(p.PartyName))
            column.Item().Text($"{p.PartyLabel}: {p.PartyName}");

        if (!string.IsNullOrWhiteSpace(p.PartyPhone))
            column.Item().Text($"Phone: {p.PartyPhone}");

        if (!string.IsNullOrWhiteSpace(p.BuyerNationalId))
            column.Item().Text($"National ID: {p.BuyerNationalId}");
    }

    private static void RenderProductInfo(ColumnDescriptor column, InvoicePresentation p)
    {
        if (!string.IsNullOrWhiteSpace(p.ProductInformation))
            column.Item().PaddingTop(8).Text(p.ProductInformation);

        column.Item().Text($"Price: {p.FinishedPrice:N0}");
        column.Item().Text($"Count: {p.ProductCount}");

        if (!string.IsNullOrWhiteSpace(p.OwnershipStatus))
            column.Item().Text($"Ownership: {p.OwnershipStatus}");

        column.Item().Text($"Date: {p.TransactionDate:yyyy-MM-dd HH:mm}");
    }

    private static void RenderGuaranteeInfo(ColumnDescriptor column, InvoicePresentation p)
    {
        foreach (var (label, value) in p.ProductExtras)
            column.Item().Text($"{label}: {value}");

        foreach (var (label, value) in p.GuaranteeInformation)
            column.Item().Text($"{label}: {value}");
    }

    private static void RenderNotesAndSignature(ColumnDescriptor column, InvoicePresentation p)
    {
        if (!string.IsNullOrWhiteSpace(p.Notes))
            column.Item().PaddingTop(8).Text(p.Notes);

        column.Item().PaddingTop(8).AlignRight().Text("Signature: ______");
    }

    private void RenderFooter(TextDescriptor text)
    {
        text.Span(_settings.ShopAddress);
        if (!string.IsNullOrWhiteSpace(_settings.ShopPhone))
            text.Span($" | {_settings.ShopPhone}");
        if (!string.IsNullOrWhiteSpace(_settings.ShopInstagram))
            text.Span($" | {_settings.ShopInstagram}");
    }

    private static PageSize ParsePageSize(string pageSize)
        => pageSize.Trim().ToUpperInvariant() switch
        {
            "A5" => PageSizes.A5,
            "LETTER" => PageSizes.Letter,
            _ => PageSizes.A4
        };

    private InvoicePresentation Resolve(InvoiceViewModel model)
    {
        var isSale = model.BuyerName is not null;
        return new InvoicePresentation
        {
            ShopName = _settings.ShopName,
            InvoiceKind = isSale ? "Sale invoice" : "Purchase invoice",
            PartyLabel = isSale ? "Customer" : "Seller",
            PartyName = isSale ? model.BuyerName : model.SellerName,
            PartyPhone = isSale ? model.BuyerPhoneNumber : model.SellerPhoneNumber,
            BuyerNationalId = isSale ? model.BuyerNationalId : null,
            ProductInformation = model.ProductInformation,
            FinishedPrice = model.FinishedPrice,
            ProductCount = model.ProductCount,
            OwnershipStatus = model.OwnershipTransferred is bool t ? (t ? "Transferred" : "Not transferred") : null,
            TransactionDate = model.TransactionDate,
            ProductExtras = model.ProductExtras,
            GuaranteeInformation = model.GuaranteeInformation,
            Notes = model.Notes
        };
    }
}

/// <summary>
/// Fully-resolved presentation model for the invoice PDF.
/// All null/empty handling and business logic is resolved here so the render methods
/// can focus purely on layout.
/// </summary>
internal sealed record InvoicePresentation
{
    public string ShopName { get; init; } = string.Empty;
    public string InvoiceKind { get; init; } = string.Empty;
    public string PartyLabel { get; init; } = string.Empty;
    public string? PartyName { get; init; }
    public string? PartyPhone { get; init; }
    public string? BuyerNationalId { get; init; }
    public string? ProductInformation { get; init; }
    public decimal FinishedPrice { get; init; }
    public int ProductCount { get; init; }
    public string? OwnershipStatus { get; init; }
    public DateTime TransactionDate { get; init; }
    public IEnumerable<(string Label, string Value)> ProductExtras { get; init; } = [];
    public IEnumerable<(string Label, string Value)> GuaranteeInformation { get; init; } = [];
    public string? Notes { get; init; }
}