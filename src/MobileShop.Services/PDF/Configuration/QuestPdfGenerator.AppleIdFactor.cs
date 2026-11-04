namespace MobileShop.Services.PDF.Configuration;

/// <summary>Apple ID delivery form layout of <see cref="QuestPdfGenerator"/>.</summary>
public sealed partial class QuestPdfGenerator
{
    private const string AppleIdFactorTitle = "فاکتور تحویل اپل آیدی";
    private const string AppleIdFactorSubtitle = "APPLE ID DELIVERY FORM";
    private const string AppleIdConfirmationText =
        "اینجانب اطلاعات فوق را صحیح دریافت کرده و مسئولیت نگهداری از Apple ID و اطلاعات آن را بر عهده دارم.";

    private const int AppleIdNotesLineCount = 3;

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
        page.DefaultTextStyle(x => x.FontFamily(FactorFontFamilies).FontSize(10).FontColor("#25313C"));

        page.Header().Element(c => c.ContentFromRightToLeft().Element(RenderAppleIdFactorHeader));
        page.Content()
            .Element(c => c.ContentFromRightToLeft().Element(x => RenderAppleIdFactorContent(x, model)));
        page.Footer().Element(c => c.ContentFromRightToLeft().Element(RenderFactorFooter));
    }

    private void RenderAppleIdFactorHeader(IContainer container) =>
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
                        column.Spacing(3);
                        column
                            .Item()
                            .AlignRight()
                            .Text(AppleIdFactorTitle)
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#102A43")
                            .FontFamily(FactorFontFamilies);
                        column
                            .Item()
                            .AlignRight()
                            .Text(AppleIdFactorSubtitle)
                            .FontSize(8)
                            .FontColor("#627D98")
                            .FontFamily(FactorFontFamilies);
                    });

                row.RelativeItem()
                    .AlignMiddle()
                    .Column(column =>
                    {
                        column.Spacing(3);
                        column
                            .Item()
                            .AlignLeft()
                            .Text(FactorShopName)
                            .FontSize(19)
                            .SemiBold()
                            .FontColor("#102A43")
                            .FontFamily(FactorFontFamilies);
                        if (!string.IsNullOrWhiteSpace(_settings.ShopAddress))
                            column
                                .Item()
                                .AlignLeft()
                                .Text(_settings.ShopAddress)
                                .FontSize(8)
                                .FontColor("#627D98")
                                .FontFamily(FactorFontFamilies);
                    });
            });

    private static void RenderAppleIdFactorContent(IContainer container, AppleIdFactorViewModel model) =>
        container
            .PaddingTop(14)
            .Column(column =>
            {
                column.Spacing(12);
                column.Item().Element(c => RenderAppleIdFactorParties(c, model));
                column.Item().Element(c => RenderAppleIdCredentialsCard(c, model));
                column.Item().Element(c => RenderAppleIdNotesCard(c, model.Notes));
                column.Item().PreventPageBreak().Element(RenderAppleIdConfirmationCard);
            });

    private static void RenderAppleIdFactorParties(IContainer container, AppleIdFactorViewModel model) =>
        container.Column(column =>
        {
            column.Spacing(8);
            column
                .Item()
                .Row(row =>
                {
                    row.RelativeItem().Element(c => AppleIdLabeledLine(c, "نام مشتری", model.CustomerName));
                    row.ConstantItem(24);
                    row.RelativeItem().Element(c => AppleIdLabeledLine(c, "تاریخ", AppleIdDateText(model.Date)));
                });
            column
                .Item()
                .Row(row =>
                {
                    row.RelativeItem()
                        .Element(c => AppleIdLabeledLine(c, "شماره تماس", model.CustomerPhone?.ToPersianDigits()));
                    row.ConstantItem(24);
                    row.RelativeItem();
                });
        });

    private static void AppleIdLabeledLine(IContainer container, string label, string? value) =>
        container.Row(row =>
        {
            row.Spacing(8);
            row.ConstantItem(62).Text(label).SemiBold().FontFamily(FactorFontFamilies);
            row.RelativeItem()
                .BorderBottom(1)
                .BorderColor("#9FB3C8")
                .PaddingBottom(2)
                .Text(AppleIdBlankIfEmpty(value))
                .FontFamily(FactorFontFamilies);
        });

    private static void RenderAppleIdCredentialsCard(IContainer container, AppleIdFactorViewModel model) =>
        AppleIdCard(
            container,
            "اطلاعات Apple ID",
            column =>
            {
                AppleIdCredentialField(column, "1. Apple ID", model.Email);
                AppleIdCredentialField(column, "2. Password", model.Password);
                AppleIdCredentialField(column, "3. Q1 (best friend)", model.SecurityAnswer1);
                AppleIdCredentialField(column, "4. Q2 (dream job)", model.SecurityAnswer2);
                AppleIdCredentialField(column, "5. Q3 (parents meet)", model.SecurityAnswer3);
                AppleIdCredentialField(column, "6. Date", AppleIdDateText(model.Date));
            });

    // The credential rows mirror the printed English form, so they read left to right.
    private static void AppleIdCredentialField(ColumnDescriptor column, string label, string? value) =>
        column
            .Item()
            .ContentFromLeftToRight()
            .Row(row =>
            {
                row.Spacing(12);
                row.ConstantItem(125).Text(label).FontFamily(FactorFontFamilies);
                row.RelativeItem()
                    .BorderBottom(1)
                    .BorderColor("#9FB3C8")
                    .PaddingBottom(2)
                    .Text(AppleIdBlankIfEmpty(value))
                    .FontFamily(FactorFontFamilies);
            });

    private static void RenderAppleIdNotesCard(IContainer container, string? notes) =>
        AppleIdCard(
            container,
            "توضیحات",
            column =>
            {
                if (!string.IsNullOrWhiteSpace(notes))
                    column.Item().Text(notes).FontFamily(FactorFontFamilies);

                for (var line = 0; line < AppleIdNotesLineCount; line++)
                    column.Item().Height(22).BorderBottom(1).BorderColor("#D9E2EC");
            });

    private static void RenderAppleIdConfirmationCard(IContainer container) =>
        AppleIdCard(
            container,
            "تایید مشتری",
            column =>
            {
                column.Item().Text(AppleIdConfirmationText).FontSize(9).FontFamily(FactorFontFamilies);
                column
                    .Item()
                    .PaddingTop(30)
                    .AlignCenter()
                    .Width(240)
                    .BorderBottom(1)
                    .BorderColor("#9FB3C8");
                column
                    .Item()
                    .AlignCenter()
                    .Text("امضاء مشتری")
                    .FontSize(8)
                    .FontColor("#627D98")
                    .FontFamily(FactorFontFamilies);
            });

    private static void AppleIdCard(IContainer container, string title, Action<ColumnDescriptor> body) =>
        container
            .Border(1)
            .BorderColor("#D9E2EC")
            .Padding(12)
            .Column(column =>
            {
                column.Spacing(10);
                column
                    .Item()
                    .AlignCenter()
                    .Text(title)
                    .FontSize(13)
                    .SemiBold()
                    .FontColor("#102A43")
                    .FontFamily(FactorFontFamilies);
                body(column);
            });

    private static string AppleIdDateText(DateTime date) => date.ToJalaliDateString().ToPersianDigits();

    // QuestPDF text blocks need content, so blank values render a space above the writing line.
    private static string AppleIdBlankIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? " " : value;
}
