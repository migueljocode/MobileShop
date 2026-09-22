namespace MobileShop.Services.PDF;

/// <summary>QuestPDF-backed invoice generator.</summary>
/// <remarks>This is the only class in the solution that references QuestPDF types.</remarks>
public sealed class QuestPdfGenerator(IOptions<PdfSettings> options) : IPdfGenerator
{
    private readonly PdfSettings _settings = options.Value;

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

            page.Header().Column(column =>
            {
                column.Item().Text(_settings.ShopName).Bold().FontSize(18);

                // the view model carries the buyer for a sale and the seller for a purchase
                column.Item().Text(model.BuyerName is null ? "Purchase invoice" : "Sale invoice").FontSize(14);
            });

            page.Content().Column(column =>
            {
                var partyLabel = model.BuyerName is null ? "Seller" : "Customer";
                var partyName = model.BuyerName ?? model.SellerName;
                var partyPhone = model.BuyerName is null ? model.SellerPhoneNumber : model.BuyerPhoneNumber;

                if (!string.IsNullOrWhiteSpace(partyName))
                    column.Item().Text($"{partyLabel}: {partyName}");

                if (!string.IsNullOrWhiteSpace(partyPhone))
                    column.Item().Text($"Phone: {partyPhone}");

                if (!string.IsNullOrWhiteSpace(model.ProductInformation))
                    column.Item().PaddingTop(8).Text(model.ProductInformation);

                column.Item().Text($"Price: {model.FinishedPrice:N0}");
                column.Item().Text($"Date: {model.TransactionDate:yyyy-MM-dd HH:mm}");

                foreach (var (label, value) in model.ProductExtras)
                    column.Item().Text($"{label}: {value}");

                foreach (var (label, value) in model.GuaranteeInformation)
                    column.Item().Text($"{label}: {value}");

                if (!string.IsNullOrWhiteSpace(model.Notes))
                    column.Item().PaddingTop(8).Text(model.Notes);
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

    private static PageSize ParsePageSize(string pageSize)
        => pageSize.Trim().ToUpperInvariant() switch
        {
            "A5" => PageSizes.A5,
            "LETTER" => PageSizes.Letter,
            _ => PageSizes.A4
        };
}
