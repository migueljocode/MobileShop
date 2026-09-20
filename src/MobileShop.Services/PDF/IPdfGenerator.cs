namespace MobileShop.Services.PDF;

/// <summary>Generates PDF documents for invoices.</summary>
public interface IPdfGenerator
{
    byte[] Generate(InvoiceViewModel model);
}
