using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MobileShop.Services.PDF.Settings;
using Microsoft.Extensions.Options;

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
                column.Item().Text(model.ShopName ?? _settings.ShopName).Bold().FontSize(18);
                column.Item().Text(model.Title).FontSize(14);
            });

            page.Content().Column(column =>
            {
                if (!string.IsNullOrWhiteSpace(model.CustomerName))
                    column.Item().Text($"Customer: {model.CustomerName}");

                if (!string.IsNullOrWhiteSpace(model.Details))
                    column.Item().PaddingTop(8).Text(model.Details);
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
