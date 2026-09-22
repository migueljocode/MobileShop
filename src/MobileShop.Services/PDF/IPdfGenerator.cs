namespace MobileShop.Services.PDF;

/// <summary>Generates PDF documents for invoices.</summary>
public interface IPdfGenerator
{
    /// <summary>
    /// Renders the provided invoice into a PDF payload.
    /// </summary>
    /// <param name="model">The invoice data to render.</param>
    /// <returns>The generated PDF bytes.</returns>
    byte[] Generate(InvoiceViewModel model);
}
