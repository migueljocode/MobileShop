namespace MobileShop.Services.PDF;

/// <summary>Generates PDF documents for invoices and transaction factors.</summary>
public interface IPdfGenerator
{
    /// <summary>
    /// Renders the provided invoice into a PDF payload.
    /// </summary>
    /// <param name="model">The invoice data to render.</param>
    /// <returns>The generated PDF bytes.</returns>
    byte[] Generate(InvoiceViewModel model);

    /// <summary>
    /// Renders a factor/report containing exactly the supplied transaction rows.
    /// </summary>
    /// <param name="model">The transaction factor data to render.</param>
    /// <returns>The generated PDF bytes.</returns>
    byte[] GenerateTransactionFactor(TransactionFactorViewModel model);
}
