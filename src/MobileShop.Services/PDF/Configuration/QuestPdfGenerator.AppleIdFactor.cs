namespace MobileShop.Services.PDF.Configuration;

/// <summary>Apple ID delivery form layout of <see cref="QuestPdfGenerator"/>.</summary>
public sealed partial class QuestPdfGenerator
{
    private const string AppleIdFactorTitle = "فاکتور تحویل اپل آیدی";
    private const string AppleIdFactorSubtitle = "APPLE ID DELIVERY FORM";
    private const string AppleIdConfirmationText =
        "اینجانب اطلاعات فوق را صحیح دریافت کرده و مسئولیت نگهداری از Apple ID و اطلاعات آن را بر عهده دارم.";

    private const string AppleIdInk = "#0F172A";
    private const string AppleIdMuted = "#64748B";
    private const string AppleIdLine = "#E2E8F0";
    private const string AppleIdSurface = "#F8FAFC";
    private const string AppleIdWhite = "#FFFFFF";
    private const string AppleIdAccent = "#0071E3";
    private const string AppleIdAccentSoft = "#EAF3FF";
    private const string AppleIdAccentBorder = "#BFDBFE";

    /// <inheritdoc />
    public byte[] GenerateAppleIdFactor(AppleIdFactorViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return Create(renderDocument).GeneratePdf();

        void renderDocument(IDocumentContainer container) => container.Page(renderPage);
        void renderPage(PageDescriptor page) => ComposeAppleIdFactorPage(page, model);
    }

    private void ComposeAppleIdFactorPage(PageDescriptor page, AppleIdFactorViewModel model)
    {
        page.Size(ParsePageSize(_settings.PageSize));
        page.MarginTop(_settings.MarginTop);
        page.MarginRight(_settings.MarginRight);
        page.MarginBottom(_settings.MarginBottom);
        page.MarginLeft(_settings.MarginLeft);
        page.DefaultTextStyle(x => x.FontFamily(FactorFontFamilies).FontSize(10).FontColor(AppleIdInk));

        page.Header()
            .PaddingBottom(14)
            .Element(c => c.ContentFromRightToLeft().Element(RenderAppleIdFactorHeader));
        page.Content()
            .Element(c => c.ContentFromRightToLeft().Element(x => RenderAppleIdFactorContent(x, model)));
        page.Footer().Element(c => c.ContentFromRightToLeft().Element(RenderAppleIdFactorFooter));
    }

    private static void RenderAppleIdFactorHeader(IContainer container) =>
        container
            .Background(AppleIdInk)
            .CornerRadius(14)
            .Padding(16)
            .Row(row =>
            {
                row.RelativeItem()
                    .AlignMiddle()
                    .Column(column =>
                    {
                        column.Spacing(4);
                        column
                            .Item()
                            .Text(AppleIdFactorSubtitle)
                            .FontSize(7.5f)
                            .FontColor("#93C5FD")
                            .FontFamily(FactorFontFamilies);
                        column
                            .Item()
                            .Text(AppleIdFactorTitle)
                            .FontSize(20)
                            .SemiBold()
                            .FontColor(AppleIdWhite)
                            .FontFamily(FactorFontFamilies);
                    });

                row.AutoItem()
                    .AlignMiddle()
                    .Column(column =>
                    {
                        column.Spacing(3);
                        column
                            .Item()
                            .AlignLeft()
                            .Text(FactorShopName)
                            .FontSize(14)
                            .SemiBold()
                            .FontColor(AppleIdWhite)
                            .FontFamily(FactorFontFamilies);
                    });
            });

    // Same address, phone and Instagram lines as the sale factor, so both documents carry identical contact details.
    private static void RenderAppleIdFactorFooter(IContainer container) =>
        container
            .BorderTop(1)
            .BorderColor(AppleIdLine)
            .Column(column => RenderFactorContactLines(column, 6));

    private static void RenderAppleIdFactorContent(IContainer container, AppleIdFactorViewModel model) =>
        container.Column(column =>
        {
            column.Spacing(12);
            column.Item().Element(c => RenderAppleIdFactorParties(c, model));
            column.Item().Element(c => RenderAppleIdCredentialsCard(c, model));
            column.Item().Element(c => RenderAppleIdNotesCard(c, model.Notes));
            column.Item().PreventPageBreak().Element(RenderAppleIdConfirmationCard);
        });

    private static void RenderAppleIdFactorParties(IContainer container, AppleIdFactorViewModel model) =>
        container.Row(row =>
        {
            row.Spacing(10);
            row.RelativeItem(1.5f).Element(c => AppleIdInfoChip(c, "نام مشتری", model.CustomerName));
            row.RelativeItem(1.1f).Element(c => AppleIdInfoChip(c, "شماره تماس", model.CustomerPhone?.ToPersianDigits()));
            row.RelativeItem().Element(c => AppleIdInfoChip(c, "تاریخ", AppleIdDateText(model.Date)));
        });

    private static void AppleIdInfoChip(IContainer container, string label, string? value) =>
        container
            .Background(AppleIdSurface)
            .Border(1)
            .BorderColor(AppleIdLine)
            .CornerRadius(10)
            .Padding(9)
            .Column(column =>
            {
                column.Spacing(3);
                column.Item().Text(label).FontSize(7.5f).FontColor(AppleIdMuted).FontFamily(FactorFontFamilies);
                column
                    .Item()
                    .MinHeight(16)
                    .Text(text =>
                    {
                        text.AlignCenter();
                        text.Span(AppleIdBlankIfEmpty(value)).FontSize(11).SemiBold().FontFamily(FactorFontFamilies);
                    });
            });

    private static void RenderAppleIdCredentialsCard(IContainer container, AppleIdFactorViewModel model) =>
        AppleIdCard(
            container,
            "اطلاعات Apple ID",
            column =>
            {
                AppleIdCredentialField(column, 1, "Apple ID", model.Email);
                AppleIdCredentialField(column, 2, "Password", model.Password);
                AppleIdCredentialField(column, 3, "Q1 (best friend)", model.SecurityAnswer1);
                AppleIdCredentialField(column, 4, "Q2 (dream job)", model.SecurityAnswer2);
                AppleIdCredentialField(column, 5, "Q3 (parents meet)", model.SecurityAnswer3);
                AppleIdCredentialField(column, 6, "Date", AppleIdDateText(model.Date));
            });

    // The credential rows mirror the English form labels, so they read left to right.
    private static void AppleIdCredentialField(ColumnDescriptor column, int number, string label, string? value) =>
        column
            .Item()
            .ContentFromLeftToRight()
            .Row(row =>
            {
                row.Spacing(10);
                row.ConstantItem(18)
                    .AlignMiddle()
                    .Height(18)
                    .Background(AppleIdAccentSoft)
                    .CornerRadius(9)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(number.ToString())
                    .FontSize(8)
                    .SemiBold()
                    .FontColor(AppleIdAccent)
                    .FontFamily(FactorFontFamilies);
                row.ConstantItem(118)
                    .AlignMiddle()
                    .Text(label)
                    .FontSize(9)
                    .FontColor(AppleIdMuted)
                    .FontFamily(FactorFontFamilies);
                row.RelativeItem()
                    .Background(AppleIdSurface)
                    .Border(1)
                    .BorderColor(AppleIdLine)
                    .CornerRadius(7)
                    .PaddingVertical(5)
                    .PaddingHorizontal(9)
                    .Text(AppleIdBlankIfEmpty(value))
                    .FontSize(10.5f)
                    .FontFamily(FactorFontFamilies);
            });

    private static void RenderAppleIdNotesCard(IContainer container, string? notes) =>
        AppleIdCard(
            container,
            "توضیحات",
            column =>
                column
                    .Item()
                    .MinHeight(60)
                    .Background(AppleIdSurface)
                    .Border(1)
                    .BorderColor(AppleIdLine)
                    .CornerRadius(10)
                    .Padding(10)
                    .Text(AppleIdBlankIfEmpty(notes))
                    .FontSize(10)
                    .FontFamily(FactorFontFamilies));

    private static void RenderAppleIdConfirmationCard(IContainer container) =>
        container
            .Background(AppleIdAccentSoft)
            .Border(1)
            .BorderColor(AppleIdAccentBorder)
            .CornerRadius(14)
            .Padding(14)
            .Column(column =>
            {
                column.Spacing(8);
                column.Item().Element(c => AppleIdSectionTitle(c, "تایید مشتری"));
                column
                    .Item()
                    .Text(text =>
                    {
                        text.AlignCenter();
                        text.Span(AppleIdConfirmationText).FontSize(9).FontColor("#1E3A5F").FontFamily(FactorFontFamilies);
                    });
                column
                    .Item()
                    .PaddingTop(4)
                    .AlignCenter()
                    .Width(250)
                    .Height(42)
                    .Background(AppleIdWhite)
                    .Border(1)
                    .BorderColor(AppleIdAccentBorder)
                    .CornerRadius(10);
                column
                    .Item()
                    .AlignCenter()
                    .Text("امضاء مشتری")
                    .FontSize(8)
                    .FontColor(AppleIdMuted)
                    .FontFamily(FactorFontFamilies);
            });

    private static void AppleIdCard(IContainer container, string title, Action<ColumnDescriptor> body) =>
        container
            .Border(1)
            .BorderColor(AppleIdLine)
            .CornerRadius(14)
            .Padding(14)
            .Column(column =>
            {
                column.Spacing(9);
                column.Item().Element(c => AppleIdSectionTitle(c, title));
                body(column);
            });

    private static void AppleIdSectionTitle(IContainer container, string title) =>
        container.Row(row =>
        {
            row.Spacing(8);
            row.ConstantItem(4).AlignMiddle().Height(16).Background(AppleIdAccent).CornerRadius(2);
            row.RelativeItem()
                .AlignMiddle()
                .Text(title)
                .FontSize(13)
                .SemiBold()
                .FontFamily(FactorFontFamilies);
        });

    private static string AppleIdDateText(DateTime date) => date.ToJalaliDateString().ToPersianDigits();

    // QuestPDF text blocks need content, so blank values render a space inside the writing box.
    private static string AppleIdBlankIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? " " : value;
}
