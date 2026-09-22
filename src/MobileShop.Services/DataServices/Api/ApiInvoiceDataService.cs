namespace MobileShop.Services.DataServices.Api;

public class ApiInvoiceDataService : IInvoiceDataService
{
    public InvoiceViewModel? GetInvoice(int transactionId)
        => throw new NotImplementedException("ApiInvoiceDataService is not implemented yet.");

    public byte[] GeneratePdf(int transactionId)
        => throw new NotImplementedException("ApiInvoiceDataService is not implemented yet.");
}
