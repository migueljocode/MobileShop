namespace MobileShop.Services.DataServices.Api;

public class ApiInvoiceDataService : IInvoiceDataService
{
    /// <inheritdoc />
    public InvoiceViewModel? GetInvoice(int transactionId)
        => throw new NotImplementedException("ApiInvoiceDataService is not implemented yet.");

    /// <inheritdoc />
    public byte[] GeneratePdf(int transactionId)
        => throw new NotImplementedException("ApiInvoiceDataService is not implemented yet.");
}
