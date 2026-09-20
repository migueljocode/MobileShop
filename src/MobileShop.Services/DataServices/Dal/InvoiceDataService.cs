namespace MobileShop.Services.DataServices.Dal;

/// <summary>Assembles invoice data and delegates PDF rendering to the PDF service.</summary>
public sealed class InvoiceDataService(
    AppDbContext context,
    IPdfGenerator pdfGenerator) : IInvoiceDataService
{
    public InvoiceViewModel? GetInvoice(int transactionId)
    {
        var transaction = context.Transactions
            .Include(t => t.SellerNavigation).ThenInclude(s => s.PersonNavigation)
            .Include(t => t.CustomerNavigation).ThenInclude(c => c.PersonNavigation)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.ModelNavigation).ThenInclude(m => m.ManufacturerNavigation)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.ModelNavigation).ThenInclude(m => m.CategoryNavigation)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.ColorNavigation)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.PhoneProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.AppleIdProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.SecondHandProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.GuaranteeProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.LaptopProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.CableProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.ChargerProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.PowerBankProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.PortableStorageProfile)
            .Include(t => t.ProductNavigation)
                .ThenInclude(p => p.PortableStorageProfile)
                .ThenInclude(s => s.StorageCapacityNavigation)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.CaseProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.GlassProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.TabletProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.SmartWatchProfile)
            .FirstOrDefault(t => t.Id == transactionId);

        if (transaction is null)
            return null;

        var product = transaction.ProductNavigation;
        var buyer = transaction.CustomerNavigation.PersonNavigation;
        var seller = transaction.SellerNavigation.PersonNavigation;
        var extras = GetProductExtras(product).ToList();
        var guarantee = product.GuaranteeProfile?.GetInvoiceExtras() ?? [];
        var productInformation = $"{product.ModelNavigation.ManufacturerNavigation.Name} {product.ModelNavigation.Name}";

        return transaction.Direction == TransactionDirection.Sell
            ? new InvoiceViewModel(
                buyer.FirstName + " " + buyer.LastName,
                transaction.CustomerNavigation.NationalId,
                buyer.PhoneNumber,
                null,
                null,
                transaction.Date,
                transaction.FinishedPrice,
                1,
                productInformation,
                extras,
                guarantee,
                product.PhoneProfile?.OwnershipTransferred,
                product.PhoneProfile?.Notes)
            : new InvoiceViewModel(
                null,
                null,
                null,
                seller.FirstName + " " + seller.LastName,
                seller.PhoneNumber,
                transaction.Date,
                transaction.FinishedPrice,
                1,
                productInformation,
                extras,
                guarantee,
                null,
                null);
    }

    public byte[] GeneratePdf(int transactionId)
        => pdfGenerator.Generate(GetInvoice(transactionId)
            ?? throw new KeyNotFoundException($"Transaction {transactionId} was not found."));

    private static IEnumerable<(string Label, string Value)> GetProductExtras(Product product)
    {
        if (product.PhoneProfile is not null)
            return product.PhoneProfile.GetInvoiceExtras();
        if (product.AppleIdProfile is not null)
            return product.AppleIdProfile.GetInvoiceExtras();
        if (product.SecondHandProfile is not null)
            return product.SecondHandProfile.GetInvoiceExtras();
        if (product.LaptopProfile is not null)
            return product.LaptopProfile.GetInvoiceExtras();
        if (product.CableProfile is not null)
            return product.CableProfile.GetInvoiceExtras();
        if (product.ChargerProfile is not null)
            return product.ChargerProfile.GetInvoiceExtras();
        if (product.PowerBankProfile is not null)
            return product.PowerBankProfile.GetInvoiceExtras();
        if (product.PortableStorageProfile is not null)
            return product.PortableStorageProfile.GetInvoiceExtras();
        if (product.CaseProfile is not null)
            return product.CaseProfile.GetInvoiceExtras();
        if (product.GlassProfile is not null)
            return product.GlassProfile.GetInvoiceExtras();
        if (product.TabletProfile is not null)
            return product.TabletProfile.GetInvoiceExtras();
        if (product.SmartWatchProfile is not null)
            return product.SmartWatchProfile.GetInvoiceExtras();
        return [];
    }
}
