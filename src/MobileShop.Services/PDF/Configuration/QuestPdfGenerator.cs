using MobileShop.Models.Extensions;

namespace MobileShop.Services.PDF.Configuration;

/// <summary>QuestPDF-backed invoice generator with English and Persian RTL support.</summary>
/// <remarks>This is the only class in the solution that references QuestPDF types.</remarks>
public sealed class QuestPdfGenerator(IOptions<PdfSettings> options) : IPdfGenerator
{
    private readonly PdfSettings _settings = options.Value;

    private const string FactorContactPhone = "۰۹۱۱۵۰۰۹۱۰۹";

    private const string InstagramGlyph =
        """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="#829AB1" stroke-width="2">
          <rect x="3" y="3" width="18" height="18" rx="5"/>
          <circle cx="12" cy="12" r="4"/>
          <circle cx="17.5" cy="6.5" r="0.6" fill="#829AB1" stroke-width="1"/>
        </svg>
        """;

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
        page.DefaultTextStyle(x => x.FontFamily("Vazirmatn").FontSize(9.5f).FontColor("#25313C"));

        page.Header()
            .Element(c => c.ContentFromRightToLeft().Element(x => RenderFactorHeader(x, model)));
        page.Content()
            .Element(c => c.ContentFromRightToLeft().Element(x => RenderFactorContent(x, model)));
        page.Footer().Element(c => c.ContentFromRightToLeft().Element(RenderFactorFooter));
    }

    private void RenderFactorHeader(IContainer container, TransactionFactorViewModel model) =>
        container
            .PaddingBottom(12)
            .BorderBottom(1.5f)
            .BorderColor("#243B53")
            .Row(row =>
            {
                row.Spacing(16);

                row.RelativeItem(1.15f)
                    .AlignMiddle()
                    .Column(column =>
                    {
                        column.Spacing(4);

                        column
                            .Item()
                            .AlignRight()
                            .Text("فاکتور تراکنش‌ها")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#102A43")
                            .FontFamily("Vazirmatn");
                        column
                            .Item()
                            .AlignRight()
                            .Text(text =>
                            {
                                text.Span("تاریخ صدور: ").SemiBold().FontFamily("Vazirmatn");
                                text.Span(
                                        model.GeneratedAt.ToJalaliDateTimeString().ToPersianDigits()
                                    )
                                    .FontFamily("Vazirmatn");
                            });
                        column
                            .Item()
                            .AlignRight()
                            .Text(text =>
                            {
                                text.Span("تعداد اقلام: ").SemiBold().FontFamily("Vazirmatn");
                                text.Span(model.Rows.Count.ToString().ToPersianDigits())
                                    .FontFamily("Vazirmatn");
                            });
                    });

                row.RelativeItem()
                    .AlignMiddle()
                    .Column(column =>
                    {
                        column.Spacing(3);
                        column
                            .Item()
                            .AlignLeft()
                            .Text(_settings.ShopName)
                            .FontSize(19)
                            .SemiBold()
                            .FontColor("#102A43")
                            .FontFamily("Vazirmatn");
                        if (!string.IsNullOrWhiteSpace(_settings.ShopAddress))
                            column
                                .Item()
                                .AlignLeft()
                                .Text(_settings.ShopAddress)
                                .FontSize(8)
                                .FontColor("#627D98")
                                .FontFamily("Vazirmatn");
                    });
            });

    private static void RenderFactorContent(
        IContainer container,
        TransactionFactorViewModel model
    ) =>
        container.Column(column =>
        {
            column.Spacing(12);
            column.Item().Element(c => RenderFactorParties(c, model));
            column.Item().Element(c => RenderFactorTable(c, model));
            column.Item().Element(c => RenderFactorSummary(c, model));
            column.Item().Element(RenderFactorSignatures);
            column.Item().Element(RenderFactorTermsSection);
        });

    private static void RenderFactorParties(
        IContainer container,
        TransactionFactorViewModel model
    ) =>
        container.Row(row =>
        {
            row.RelativeItem()
                .Element(c => FactorInfoCard(c, "خریدار / مشتری", FindParty(model, "Customer")));
            row.ConstantItem(10);
            row.RelativeItem()
                .Element(c => FactorInfoCard(c, "فروشنده", FindParty(model, "Seller")));
        });

    private static string FindParty(TransactionFactorViewModel model, string role) =>
        string.Join(
            "، ",
            model
                .Rows.Where(x =>
                    string.Equals(x.PersonRole, role, StringComparison.OrdinalIgnoreCase)
                )
                .Select(x => x.PersonLabel)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
        )
            is { Length: > 0 } value
            ? value
            : "در ردیف‌های فاکتور مشخص شده است";

    private static void FactorInfoCard(IContainer container, string title, string value) =>
        container
            .Border(1)
            .BorderColor("#D9E2EC")
            .Background("#F8FAFC")
            .Padding(7)
            .Column(column =>
            {
                column.Item().Text(title).FontSize(8).SemiBold().FontColor("#627D98");
                column
                    .Item()
                    .PaddingTop(3)
                    .Text(value)
                    .FontSize(10)
                    .SemiBold()
                    .FontColor("#243B53");
            });

    private static void RenderFactorTable(IContainer container, TransactionFactorViewModel model) =>
        container.Table(table =>
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
                HeaderCell(header.Cell(), "مبلغ (ریال)");
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

                BodyCell(
                    table.Cell(),
                    row.FinishedPrice.ToGroupedDigits().ToPersianDigits(),
                    background,
                    true
                );
                BodyCell(table.Cell(), row.PersonLabel, background);
                BodyCell(table.Cell(), row.ProductLabel, background);
                BodyCell(table.Cell(), DirectionLabel(row.Direction), background);
                BodyCell(
                    table.Cell(),
                    row.Date.ToJalaliDateTimeString().ToPersianDigits(),
                    background,
                    false,
                    true
                );
                BodyCell(
                    table.Cell(),
                    (index + 1).ToString().ToPersianDigits(),
                    background,
                    false,
                    true
                );
            }
        });

    private static void HeaderCell(IContainer cell, string text) =>
        cell.Background("#243B53")
            .PaddingVertical(4)
            .PaddingHorizontal(6)
            .AlignMiddle()
            .AlignCenter()
            .Text(text)
            .FontSize(8.5f)
            .SemiBold()
            .FontColor("#FFFFFF")
            .FontFamily("Vazirmatn")
            .AlignCenter();

    private static void BodyCell(
        IContainer cell,
        string text,
        string background,
        bool amount = false,
        bool numeric = false
    )
    {
        var content = cell.Background(background)
            .BorderBottom(1)
            .BorderColor("#E5E7EB")
            .PaddingVertical(8)
            .PaddingHorizontal(7)
            .AlignMiddle()
            .AlignCenter();

        if (amount || numeric)
            content = content.ContentFromLeftToRight();

        var label = content
            .Text(text)
            .FontSize(amount ? 8.5f : 8)
            .AlignCenter()
            .FontFamily("Vazirmatn");

        if (amount)
            label.SemiBold();
    }

    private static string DirectionLabel(TransactionDirection direction) =>
        direction == TransactionDirection.Buy ? "خرید" : "فروش";

    private static void RenderFactorSummary(
        IContainer container,
        TransactionFactorViewModel model
    ) =>
        container.Row(row =>
        {
            row.RelativeItem();
            row.ConstantItem(280)
                .Background("#F1F5F9")
                .Border(1)
                .BorderColor("#CBD5E1")
                .PaddingVertical(8)
                .PaddingHorizontal(12)
                .AlignMiddle()
                .Row(summary =>
                {
                    summary.Spacing(8);
                    summary
                        .RelativeItem()
                        .AlignRight()
                        .Text("جمع کل")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor("#102A43")
                        .FontFamily("Vazirmatn");
                    summary
                        .RelativeItem()
                        .ContentFromLeftToRight()
                        .AlignLeft()
                        .Text(text =>
                        {
                            text.Span(model.TotalPrice.ToGroupedDigits().ToPersianDigits())
                                .FontSize(13)
                                .SemiBold()
                                .FontColor("#102A43")
                                .FontFamily("Vazirmatn");
                            text.Span(" ریال")
                                .FontSize(9)
                                .FontColor("#627D98")
                                .FontFamily("Vazirmatn");
                        });
                });
        });

    private static void RenderFactorSignatures(IContainer container) =>
        container
            .PaddingTop(2)
            .PreventPageBreak()
            .Row(row =>
            {
                row.RelativeItem().Element(c => SignatureBox(c, "امضای طرف معامله"));
                row.ConstantItem(24);
                row.RelativeItem().Element(c => SignatureBox(c, "مهر و امضای فروشگاه"));
            });

    private static void SignatureBox(IContainer container, string title) =>
        container
            .Height(48)
            .Border(1)
            .BorderColor("#D9E2EC")
            .Padding(7)
            .Column(column =>
            {
                column.Item().Text(title).FontSize(8).SemiBold().FontColor("#627D98");
                column.Item().PaddingTop(16).BorderBottom(1).BorderColor("#9FB3C8");
            });

    private void RenderFactorFooter(IContainer container) =>
        container
            .PaddingTop(7)
            .BorderTop(1)
            .BorderColor("#D9E2EC")
            .AlignCenter()
            .Text(text =>
            {
                if (!string.IsNullOrWhiteSpace(_settings.ShopAddress))
                    text.Span(_settings.ShopAddress).FontSize(7.5f).FontColor("#829AB1");

                if (!string.IsNullOrWhiteSpace(_settings.ShopPhone))
                {
                    if (!string.IsNullOrWhiteSpace(_settings.ShopAddress))
                        text.Span("  |  ").FontSize(7.5f).FontColor("#829AB1");
                    text.Span(_settings.ShopPhone.ToRtlDisplayPhone()).FontSize(7.5f).FontColor("#829AB1");
                }

                if (!string.IsNullOrWhiteSpace(_settings.ShopInstagram))
                {
                    if (
                        !string.IsNullOrWhiteSpace(_settings.ShopAddress)
                        || !string.IsNullOrWhiteSpace(_settings.ShopPhone)
                    )
                        text.Span("  |  ").FontSize(7.5f).FontColor("#829AB1");
                    text.Element().PaddingBottom(-1.5f).Width(8).Height(8).Svg(InstagramGlyph);
                    text.Span(" ").FontSize(7.5f);
                    text.Span(_settings.ShopInstagram).FontSize(7.5f).FontColor("#829AB1");
                }
            });

    private static void RenderFactorTermsSection(IContainer container) =>
        container.Column(column =>
        {
            column.Spacing(4);
            column
                .Item()
                .Text("خریدار محترم لطفا موارد زیر را به دقت مطالعه و سپس امضا فرمایید")
                .FontSize(9)
                .SemiBold()
                .FontColor("#102A43")
                .FontFamily("Vazirmatn");

            foreach (var line in FactorTermsLines)
                column.Item().Text(line).FontSize(8).FontColor("#25313C").FontFamily("Vazirmatn");

            column
                .Item()
                .PaddingTop(4)
                .Text(text =>
                {
                    text.Span("گوشی کاملا تست و تحویل اینجانب ")
                        .FontSize(8)
                        .FontFamily("Vazirmatn")
                        .FontColor("#25313C");
                    text.Span("..................................")
                        .FontSize(8)
                        .FontFamily("Vazirmatn")
                        .FontColor("#627D98");
                    text.Span(" گردید.").FontSize(8).FontFamily("Vazirmatn").FontColor("#25313C");
                });

            column
                .Item()
                .PaddingTop(20)
                .AlignCenter()
                .Text("گنبدکاووس٬ خیابان طالقانی شرقی٬ طالقانی دوم ٬ زون موبایل")
                .FontSize(8)
                .FontColor("#627D98")
                .FontFamily("Vazirmatn");
            column
                .Item()
                .AlignCenter()
                .Text(text =>
                {
                    text.Span("شماره تماس: ")
                        .FontSize(8)
                        .FontColor("#627D98")
                        .FontFamily("Vazirmatn");
                    text.Span(FactorContactPhone.ToRtlDisplayPhone())
                        .FontSize(8)
                        .FontColor("#627D98")
                        .FontFamily("Vazirmatn");
                });
            column
                .Item()
                .AlignCenter()
                .ContentFromLeftToRight()
                .Text("@zonemobile.ir")
                .FontSize(8)
                .FontColor("#627D98")
                .FontFamily("Vazirmatn");
        });

    private static readonly string[] FactorTermsLines =
    [
        "۱. مورد معامله صحیح و سالم تحویل خریدار گردید و خریدار موظف است قبل از تغییرات قیمت را بررسی فرماید. فروشنده در قبال نوسانات بازار مسئول نمیباشد.",
        "۲. تا تسویه حساب کامل دستگاه ها به صورت امانت نزد خریدار میباشد.",
        "۳. این معامله بر اساس ماده ۱۰ آیین نامه قانون مدنی تنظیم گردیده و در صورت فسخ معامله خسارت برابر عرف بازار و اعلام نظر کارشناس اتحادیه به عهده خریدار میباشد.",
        "۴. کلیه موارد فوق به رویت خریدار رسیده و خریدار هیچگاه اعتراضی ندارد.",
        "۵. به دلیل الکترونیکی بودن کالا در هنگام خرید دقت کامل فرمایید پس از انجام معامله مرجوع یا تعویض نمیشود.",
        "۶. ارسال گوشی جهت گارانتی به عهده مشتری میباشد.",
    ];

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

        column.Item().Text($"Price: {p.FinishedPrice.ToGroupedDigits()} Toman");
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

    private static void RenderPartyInfoPersian(
        ColumnDescriptor column,
        PersianInvoicePresentation p
    )
    {
        if (!string.IsNullOrWhiteSpace(p.PartyName))
            column.Item().Text($"{p.PartyLabel} : {p.PartyName}").FontFamily("Vazirmatn");

        if (!string.IsNullOrWhiteSpace(p.PartyPhone))
            column.Item().Text($"تلفن : {p.PartyPhone}").FontFamily("Vazirmatn");

        if (!string.IsNullOrWhiteSpace(p.BuyerNationalId))
            column.Item().Text($"کد ملی : {p.BuyerNationalId}").FontFamily("Vazirmatn");
    }

    private static void RenderProductInfoPersian(
        ColumnDescriptor column,
        PersianInvoicePresentation p
    )
    {
        if (!string.IsNullOrWhiteSpace(p.ProductInformation))
            column.Item().PaddingTop(8).Text(p.ProductInformation).FontFamily("Vazirmatn");

        column
            .Item()
            .Text($"قیمت : {p.FinishedPrice.ToGroupedDigits()} ریال")
            .FontFamily("Vazirmatn");
        column.Item().Text($"تعداد : {p.ProductCount}").FontFamily("Vazirmatn");

        if (!string.IsNullOrWhiteSpace(p.OwnershipStatus))
            column.Item().Text($"انتقال مالکیت : {p.OwnershipStatus}").FontFamily("Vazirmatn");

        column.Item().Text($"تاریخ : {p.TransactionDate:yyyy/MM/dd HH:mm}").FontFamily("Vazirmatn");
    }

    private static void RenderGuaranteeInfoPersian(
        ColumnDescriptor column,
        PersianInvoicePresentation p
    )
    {
        foreach (var (label, value) in p.ProductExtras)
            column.Item().Text($"{label} : {value}").FontFamily("Vazirmatn");

        foreach (var (label, value) in p.GuaranteeInformation)
            column.Item().Text($"{label} : {value}").FontFamily("Vazirmatn");
    }

    private static void RenderNotesAndSignaturePersian(
        ColumnDescriptor column,
        PersianInvoicePresentation p
    )
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

    private static PageSize ParsePageSize(string pageSize) =>
        pageSize.Trim().ToUpperInvariant() switch
        {
            "A5" => PageSizes.A5,
            "LETTER" => PageSizes.Letter,
            _ => PageSizes.A4,
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
            OwnershipStatus = model.OwnershipTransferred is bool t
                ? (t ? "Transferred" : "Not transferred")
                : null,
            TransactionDate = model.TransactionDate,
            ProductExtras = model.ProductExtras,
            GuaranteeInformation = model.GuaranteeInformation,
            Notes = model.Notes,
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
            OwnershipStatus = model.OwnershipTransferred is bool t
                ? (t ? "انتقال یافته" : "انتقال نیافته")
                : null,
            TransactionDate = model.TransactionDate,
            ProductExtras = model.ProductExtras,
            GuaranteeInformation = model.GuaranteeInformation,
            Notes = model.Notes,
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
    public long FinishedPrice { get; init; }
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
    public long FinishedPrice { get; init; }
    public int ProductCount { get; init; }
    public string? OwnershipStatus { get; init; }
    public DateTime TransactionDate { get; init; }
    public IEnumerable<(string Label, string Value)> ProductExtras { get; init; } = [];
    public IEnumerable<(string Label, string Value)> GuaranteeInformation { get; init; } = [];
    public string? Notes { get; init; }
}
