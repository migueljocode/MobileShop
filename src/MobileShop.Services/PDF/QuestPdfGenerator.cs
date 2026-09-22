namespace MobileShop.Services.PDF;

/// <summary>QuestPDF-backed invoice generator.</summary>
/// <remarks>This is the only class in the solution that references QuestPDF types.</remarks>
public sealed class QuestPdfGenerator(IOptions<PdfSettings> options) : IPdfGenerator
{
    private readonly PdfSettings _settings = options.Value;

    /// <inheritdoc />
    public byte[] Generate(InvoiceViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return Create(renderDocument).GeneratePdf();

        void renderDocument(IDocumentContainer container) => container.Page(renderPage);
        void renderPage(PageDescriptor page) => ComposePage(page, model);
        /// <summary>Applies the page settings and lays out the header, content and footer elements.</summary>
        void ComposePage(PageDescriptor page, InvoiceViewModel model)
        {
            page.Size(ParsePageSize(_settings.PageSize));
            page.MarginTop(_settings.MarginTop);
            page.MarginRight(_settings.MarginRight);
            page.MarginBottom(_settings.MarginBottom);
            page.MarginLeft(_settings.MarginLeft);
            page.Header().Column(renderHeader);
            page.Content().Column(renderContent);
            page.Footer().AlignCenter().Text(RenderFooter);

            static PageSize ParsePageSize(string pageSize)
                => pageSize.Trim().ToUpperInvariant() switch
                {
                    "A5" => PageSizes.A5,
                    "LETTER" => PageSizes.Letter,
                    _ => PageSizes.A4
                };
            void renderHeader(ColumnDescriptor column) => RenderHeader(column, model);
            void RenderHeader(ColumnDescriptor column, InvoiceViewModel model)
            {
                column.Item().Text(_settings.ShopName).Bold().FontSize(18);
                column.Item().Text(model.BuyerName is null ? "Purchase invoice" : "Sale invoice").FontSize(14);
            }
            void renderContent(ColumnDescriptor column) => RenderContent(column, model);
            static void RenderContent(ColumnDescriptor column, InvoiceViewModel model)
            {
                RenderPartyInfo(column, model);
                RenderProductInfo(column, model);
                RenderGuaranteeInfo(column, model);
                RenderNotesAndSignature(column, model);
                static void RenderPartyInfo(ColumnDescriptor column, InvoiceViewModel model)
                {
                    var partyLabel = model.BuyerName is null ? "Seller" : "Customer";
                    var partyName = model.BuyerName ?? model.SellerName;
                    var partyPhone = model.BuyerName is null ? model.SellerPhoneNumber : model.BuyerPhoneNumber;

                    if (!string.IsNullOrWhiteSpace(partyName))
                        column.Item().Text($"{partyLabel}: {partyName}");

                    if (!string.IsNullOrWhiteSpace(partyPhone))
                        column.Item().Text($"Phone: {partyPhone}");

                    if (!string.IsNullOrWhiteSpace(model.BuyerNationalId) && model.BuyerName is not null)
                        column.Item().Text($"National ID: {model.BuyerNationalId}");
                }
                static void RenderProductInfo(ColumnDescriptor column, InvoiceViewModel model)
                {
                    if (!string.IsNullOrWhiteSpace(model.ProductInformation))
                        column.Item().PaddingTop(8).Text(model.ProductInformation);

                    column.Item().Text($"Price: {model.FinishedPrice:N0}");
                    column.Item().Text($"Count: {model.ProductCount}");

                    if (model.OwnershipTransferred is bool transferred)
                        column.Item().Text($"Ownership: {(transferred ? "Transferred" : "Not transferred")}");

                    column.Item().Text($"Date: {model.TransactionDate:yyyy-MM-dd HH:mm}");
                }
                static void RenderGuaranteeInfo(ColumnDescriptor column, InvoiceViewModel model)
                {
                    foreach (var (label, value) in model.ProductExtras)
                        column.Item().Text($"{label}: {value}");

                    foreach (var (label, value) in model.GuaranteeInformation)
                        column.Item().Text($"{label}: {value}");
                }
                static void RenderNotesAndSignature(ColumnDescriptor column, InvoiceViewModel model)
                {
                    if (!string.IsNullOrWhiteSpace(model.Notes))
                        column.Item().PaddingTop(8).Text(model.Notes);

                    column.Item().PaddingTop(8).AlignRight().Text("Signature: ______");
                }
            }
            /// <summary>Renders the shop contact details, one span per configured value.</summary>
            void RenderFooter(TextDescriptor text)
            {
                text.Span(_settings.ShopAddress);
                if (!string.IsNullOrWhiteSpace(_settings.ShopPhone))
                    text.Span($" | {_settings.ShopPhone}");
                if (!string.IsNullOrWhiteSpace(_settings.ShopInstagram))
                    text.Span($" | {_settings.ShopInstagram}");
            }
        }
    }
}