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

        return Document.Create(container => container.Page(page =>
        {
            page.Size(ParsePageSize(_settings.PageSize));
            page.MarginTop(_settings.MarginTop);
            page.MarginRight(_settings.MarginRight);
            page.MarginBottom(_settings.MarginBottom);
            page.MarginLeft(_settings.MarginLeft);

            page.Header().Column(column => RenderHeader(column, model));

            page.Content().Column(column =>
            {
                RenderPartyInfo(column, model);
                RenderProductInfo(column, model);
                RenderGuaranteeInfo(column, model);
                RenderNotesAndSignature(column, model);
            });

            page.Footer().AlignCenter().Text(text =>
            {
                text.Span(_settings.ShopAddress);
                if (!string.IsNullOrWhiteSpace(_settings.ShopPhone))
                    text.Span($" | {_settings.ShopPhone}");
                if (!string.IsNullOrWhiteSpace(_settings.ShopInstagram))
                    text.Span($" | {_settings.ShopInstagram}");
            });
        })).GeneratePdf();
    }

    private void RenderHeader(ColumnDescriptor column, InvoiceViewModel model)
    {
        column.Item().Text(_settings.ShopName).Bold().FontSize(18);
        column.Item().Text(model.BuyerName is null ? "Purchase invoice" : "Sale invoice").FontSize(14);
    }

    private static void RenderPartyInfo(ColumnDescriptor column, InvoiceViewModel model)
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

    private static void RenderProductInfo(ColumnDescriptor column, InvoiceViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.ProductInformation))
            column.Item().PaddingTop(8).Text(model.ProductInformation);

        column.Item().Text($"Price: {model.FinishedPrice:N0}");
        column.Item().Text($"Count: {model.ProductCount}");

        if (model.OwnershipTransferred is bool transferred)
            column.Item().Text($"Ownership: {(transferred ? "Transferred" : "Not transferred")}");

        column.Item().Text($"Date: {model.TransactionDate:yyyy-MM-dd HH:mm}");
    }

    private static void RenderGuaranteeInfo(ColumnDescriptor column, InvoiceViewModel model)
    {
        foreach (var (label, value) in model.ProductExtras)
            column.Item().Text($"{label}: {value}");

        foreach (var (label, value) in model.GuaranteeInformation)
            column.Item().Text($"{label}: {value}");
    }

    private static void RenderNotesAndSignature(ColumnDescriptor column, InvoiceViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Notes))
            column.Item().PaddingTop(8).Text(model.Notes);

        column.Item().PaddingTop(8).AlignRight().Text("Signature: ______");
    }

    private static PageSize ParsePageSize(string pageSize)
        => pageSize.Trim().ToUpperInvariant() switch
        {
            "A5" => PageSizes.A5,
            "LETTER" => PageSizes.Letter,
            _ => PageSizes.A4
        };
}
