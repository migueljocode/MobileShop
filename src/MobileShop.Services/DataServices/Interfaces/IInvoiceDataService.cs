namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>Builds invoices from persisted transactions and generates their PDFs.</summary>
public interface IInvoiceDataService
{
    InvoiceViewModel? GetInvoice(int transactionId);
    byte[] GeneratePdf(int transactionId);
}
