namespace MobileShop.Services.PDF.Configuration;

/// <summary>QuestPDF-backed invoice generator with English and Persian RTL support.</summary>
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

    /// <summary>
    /// Generates a Persian (RTL) invoice PDF using Vazirmatn font.
    /// </summary>
    public byte[] GeneratePersian(InvoiceViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var presentation = ResolvePersian(model);
        return Create(renderDocument).GeneratePdf();

        void renderDocument(IDocumentContainer container) => container.Page(renderPage);
        void renderPage(PageDescriptor page) => ComposePagePersian(page, presentation);
    }

    /// <inheritdoc />
    public byte[] GenerateTransactionFactor(TransactionFactorViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return Create(renderDocument).GeneratePdf();

        void renderDocument(IDocumentContainer container) => container.Page(renderPage);
        void renderPage(PageDescriptor page) => ComposeFactorPage(page, model);
    }

    private void ComposeFactorPage(PageDescriptor page, TransactionFactorViewModel model)
    {
        page.Size(ParsePageSize(_settings.PageSize));
        page.MarginTop(_settings.MarginTop);
        page.MarginRight(_settings.MarginRight);
        page.MarginBottom(_settings.MarginBottom);
        page.MarginLeft(_settings.MarginLeft);
        page.DefaultTextStyle(x => x.FontFamily("Vazirmatn").FontSize(9).FontColor("#25313C"));

        page.Header().Element(c => RenderFactorHeader(c, model));
        page.Content().Element(c => RenderFactorContent(c, model));
        page.Footer().Element(RenderFactorFooter);
    }

    private void RenderFactorHeader(IContainer container, TransactionFactorViewModel model)
        => container
            .PaddingBottom(14)
            .BorderBottom(1.5f)
            .BorderColor("#243B53")
            .Row(row =>
            {
                row.RelativeItem().AlignLeft().Column(column =>
                {
                    column.Item().Text(_settings.ShopName).FontSize(19).SemiBold().FontColor("#102A43");
                    if (!string.IsNullOrWhiteSpace(_settings.ShopAddress))
                        column.Item().PaddingTop(3).Text(_settings.ShopAddress).FontSize(8).FontColor("#627D98");
                });

                row.ConstantItem(190).AlignRight().Column(column =>
                {
                    column.Item().AlignRight().Text("فاکتور تراکنش‌ها").FontSize(20).SemiBold().FontColor("#102A43");
                    column.Item().PaddingTop(4).AlignRight().Text(text =>
                    {
                        text.Span("تاریخ صدور: ").SemiBold();
                        text.Span(model.GeneratedAt.ToString("yyyy/MM/dd HH:mm"));
                    });
                    column.Item().PaddingTop(2).AlignRight().Text(text =>
                    {
                        text.Span("تعداد اقلام: ").SemiBold();
                        text.Span(model.Rows.Count.ToString());
                    });
                });
            });

    private static void RenderFactorContent(IContainer container, TransactionFactorViewModel model)
        => container.Column(column =>
        {
            column.Spacing(16);
            column.Item().Element(c => RenderFactorParties(c, model));
            column.Item().Element(c => RenderFactorTable(c, model));
            column.Item().Element(c => RenderFactorSummary(c, model));
            column.Item().Element(RenderFactorSignatures);
        });

    private static void RenderFactorParties(IContainer container, TransactionFactorViewModel model)
        => container.Row(row =>
        {
            row.RelativeItem().Element(c => FactorInfoCard(c, "خریدار / مشتری", FindParty(model, "Customer")));
            row.ConstantItem(10);
            row.RelativeItem().Element(c => FactorInfoCard(c, "فروشنده", FindParty(model, "Seller")));
        });

    private static string FindParty(TransactionFactorViewModel model, string role)
        => model.Rows
            .Where(x => string.Equals(x.PersonRole, role, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.PersonLabel)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            ?? "در ردیف‌های فاکتور مشخص شده است";

    private static void FactorInfoCard(IContainer container, string title, string value)
        => container
            .Border(1)
            .BorderColor("#D9E2EC")
            .Background("#F8FAFC")
            .Padding(10)
            .Column(column =>
            {
                column.Item().Text(title).FontSize(8).SemiBold().FontColor("#627D98");
                column.Item().PaddingTop(3).Text(value).FontSize(10).SemiBold().FontColor("#243B53");
            });

    private static void RenderFactorTable(IContainer container, TransactionFactorViewModel model)
        => container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(105);
                columns.RelativeColumn(1.15f);
                columns.RelativeColumn(2.25f);
                columns.ConstantColumn(60);
                columns.ConstantColumn(85);
                columns.ConstantColumn(35);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "مبلغ");
                HeaderCell(header.Cell(), "طرف معامله");
                HeaderCell(header.Cell(), "محصول");
                HeaderCell(header.Cell(), "نوع");
                HeaderCell(header.Cell(), "تاریخ");
                HeaderCell(header.Cell(), "#");
            });

            for (var index = 0; index < model.Rows.Count; index++)
            {
                var row = model.Rows[index];
                var background = index % 2 == 0 ? "#FFFFFF" : "#F8FAFC";

                BodyCell(table.Cell(), row.FinishedPrice.ToString("N0"), background, true);
                BodyCell(table.Cell(), row.PersonLabel, background);
                BodyCell(table.Cell(), row.ProductLabel, background);
                BodyCell(table.Cell(), DirectionLabel(row.Direction), background, false, true);
                BodyCell(table.Cell(), row.Date.ToString("yyyy/MM/dd HH:mm"), background, false, true);
                BodyCell(table.Cell(), (index + 1).ToString(), background, false, true);
            }
        });

    private static void HeaderCell(IContainer cell, string text)
        => cell
            .Background("#243B53")
            .PaddingVertical(8)
            .PaddingHorizontal(7)
            .AlignMiddle()
            .Text(text)
            .FontSize(8)
            .SemiBold()
            .FontColor("#FFFFFF")
            .AlignCenter();

    private static void BodyCell(
        IContainer cell,
        string text,
        string background,
        bool amount = false,
        bool centered = false)
    {
        var content = cell
            .Background(background)
            .BorderBottom(1)
            .BorderColor("#E5E7EB")
            .PaddingVertical(8)
            .PaddingHorizontal(7)
            .AlignMiddle();

        if (amount)
            content.ContentFromLeftToRight().Text(text).FontSize(8.5f).SemiBold();
        else if (centered)
            content.ContentFromLeftToRight().AlignCenter().Text(text).FontSize(8);
        else
            content.AlignLeft().Text(text).FontSize(8);
    }

    private static string DirectionLabel(TransactionDirection direction)
        => direction == TransactionDirection.Buy ? "خرید" : "فروش";

    private static void RenderFactorSummary(IContainer container, TransactionFactorViewModel model)
        => container.Row(row =>
        {
            row.RelativeItem();
            row.ConstantItem(250)
                .Background("#F1F5F9")
                .Border(1)
                .BorderColor("#CBD5E1")
                .Padding(12)
                .Row(summary =>
                {
                    summary.RelativeItem().Text("جمع کل").FontSize(11).SemiBold().FontColor("#102A43");
                    summary.ConstantItem(120).AlignRight().Text(text =>
                    {
                        text.Span(model.TotalPrice.ToString("N0")).FontSize(13).SemiBold().FontColor("#102A43");
                        text.Span("  ریال").FontSize(9).FontColor("#627D98");
                    });
                });
        });

    private static void RenderFactorSignatures(IContainer container)
        => container
            .PaddingTop(4)
            .Row(row =>
            {
                row.RelativeItem().Element(c => SignatureBox(c, "امضای طرف معامله"));
                row.ConstantItem(24);
                row.RelativeItem().Element(c => SignatureBox(c, "مهر و امضای فروشگاه"));
            });

    private static void SignatureBox(IContainer container, string title)
        => container
            .Height(62)
            .Border(1)
            .BorderColor("#D9E2EC")
            .Padding(9)
            .Column(column =>
            {
                column.Item().Text(title).FontSize(8).SemiBold().FontColor("#627D98");
                column.Item().PaddingTop(23).BorderBottom(1).BorderColor("#9FB3C8");
            });

    private void RenderFactorFooter(IContainer container)
        => container
            .PaddingTop(7)
            .BorderTop(1)
            .BorderColor("#D9E2EC")
            .AlignCenter()
            .Text(text =>
            {
                if (!string.IsNullOrWhiteSpace(_settings.ShopAddress))
                    text.Span(_settings.ShopAddress);

                if (!string.IsNullOrWhiteSpace(_settings.ShopPhone))
                {
                    if (!string.IsNullOrWhiteSpace(_settings.ShopAddress))
                        text.Span("  |  ");
                    text.Span(_settings.ShopPhone);
                }

                if (!string.IsNullOrWhiteSpace(_settings.ShopInstagram))
                {
                    if (!string.IsNullOrWhiteSpace(_settings.ShopAddress) || !string.IsNullOrWhiteSpace(_settings.ShopPhone))
                        text.Span("  |  ");
                    text.Span(_settings.ShopInstagram);
                }
            })
            .FontSize(7.5f)
            .FontColor("#829AB1");


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

    private void ComposePagePersian(PageDescriptor page, PersianInvoicePresentation p)
    {
        page.Size(ParsePageSize(_settings.PageSize));
        page.MarginTop(_settings.MarginTop);
        page.MarginRight(_settings.MarginRight);
        page.MarginBottom(_settings.MarginBottom);
        page.MarginLeft(_settings.MarginLeft);

        page.DefaultTextStyle(x => x.FontFamily("Vazirmatn").FontSize(11));

        page.Header().Column(c => RenderHeaderPersian(c, p));
        page.Content().Column(c => RenderContentPersian(c, p));
        page.Footer().AlignCenter().Text(t => RenderFooterPersian(t));
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

    private static void RenderHeaderPersian(ColumnDescriptor column, PersianInvoicePresentation p)
    {
        column.Item().Text(p.ShopName).Bold().FontSize(18).FontFamily("Vazirmatn");
        column.Item().Text(p.InvoiceKind).FontSize(14).FontFamily("Vazirmatn");
    }

    private void RenderContentPersian(ColumnDescriptor column, PersianInvoicePresentation p)
    {
        RenderPartyInfoPersian(column, p);
        RenderProductInfoPersian(column, p);
        RenderGuaranteeInfoPersian(column, p);
        RenderNotesAndSignaturePersian(column, p);
    }

    private static void RenderPartyInfoPersian(ColumnDescriptor column, PersianInvoicePresentation p)
    {
        if (!string.IsNullOrWhiteSpace(p.PartyName))
            column.Item().Text($"{p.PartyLabel} : {p.PartyName}").FontFamily("Vazirmatn");

        if (!string.IsNullOrWhiteSpace(p.PartyPhone))
            column.Item().Text($"تلفن : {p.PartyPhone}").FontFamily("Vazirmatn");

        if (!string.IsNullOrWhiteSpace(p.BuyerNationalId))
            column.Item().Text($"کد ملی : {p.BuyerNationalId}").FontFamily("Vazirmatn");
    }

    private static void RenderProductInfoPersian(ColumnDescriptor column, PersianInvoicePresentation p)
    {
        if (!string.IsNullOrWhiteSpace(p.ProductInformation))
            column.Item().PaddingTop(8).Text(p.ProductInformation).FontFamily("Vazirmatn");

        column.Item().Text($"قیمت : {p.FinishedPrice:N0}").FontFamily("Vazirmatn");
        column.Item().Text($"تعداد : {p.ProductCount}").FontFamily("Vazirmatn");

        if (!string.IsNullOrWhiteSpace(p.OwnershipStatus))
            column.Item().Text($"انتقال مالکیت : {p.OwnershipStatus}").FontFamily("Vazirmatn");

        column.Item().Text($"تاریخ : {p.TransactionDate:yyyy/MM/dd HH:mm}").FontFamily("Vazirmatn");
    }

    private static void RenderGuaranteeInfoPersian(ColumnDescriptor column, PersianInvoicePresentation p)
    {
        foreach (var (label, value) in p.ProductExtras)
            column.Item().Text($"{label} : {value}").FontFamily("Vazirmatn");

        foreach (var (label, value) in p.GuaranteeInformation)
            column.Item().Text($"{label} : {value}").FontFamily("Vazirmatn");
    }

    private static void RenderNotesAndSignaturePersian(ColumnDescriptor column, PersianInvoicePresentation p)
    {
        if (!string.IsNullOrWhiteSpace(p.Notes))
            column.Item().PaddingTop(8).Text(p.Notes).FontFamily("Vazirmatn");

        column.Item().PaddingTop(8).AlignRight().Text("امضا : ______").FontFamily("Vazirmatn");
    }

    private void RenderFooterPersian(TextDescriptor text)
    {
        text.Span(_settings.ShopAddress).FontFamily("Vazirmatn");
        if (!string.IsNullOrWhiteSpace(_settings.ShopPhone))
            text.Span($" | {_settings.ShopPhone}").FontFamily("Vazirmatn");
        if (!string.IsNullOrWhiteSpace(_settings.ShopInstagram))
            text.Span($" | {_settings.ShopInstagram}").FontFamily("Vazirmatn");
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

    private PersianInvoicePresentation ResolvePersian(InvoiceViewModel model)
    {
        var isSale = model.BuyerName is not null;
        return new PersianInvoicePresentation
        {
            ShopName = _settings.ShopName,
            InvoiceKind = isSale ? "فاکتور فروش" : "فاکتور خرید",
            PartyLabel = isSale ? "مشتری" : "فروشنده",
            PartyName = isSale ? model.BuyerName : model.SellerName,
            PartyPhone = isSale ? model.BuyerPhoneNumber : model.SellerPhoneNumber,
            BuyerNationalId = isSale ? model.BuyerNationalId : null,
            ProductInformation = model.ProductInformation,
            FinishedPrice = model.FinishedPrice,
            ProductCount = model.ProductCount,
            OwnershipStatus = model.OwnershipTransferred is bool t ? (t ? "انتقال یافته" : "انتقال نیافته") : null,
            TransactionDate = model.TransactionDate,
            ProductExtras = model.ProductExtras,
            GuaranteeInformation = model.GuaranteeInformation,
            Notes = model.Notes
        };
    }
}

/// <summary>
/// Fully-resolved presentation model for the English invoice PDF.
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

/// <summary>
/// Fully-resolved presentation model for the Persian (RTL) invoice PDF.
/// </summary>
internal sealed record PersianInvoicePresentation
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
