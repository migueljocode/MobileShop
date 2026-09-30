namespace MobileShop.Services.DataServices.Api;

public class ApiTransactionsDataService : ITransactionsDataService
{
    /// <inheritdoc />
    public Task<IReadOnlyList<TransactionListItemViewModel>> GetListAsync(string? direction, int take, bool ascending)
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<TransactionDetailsViewModel?> GetDetailsAsync(int id)
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<byte[]?> GetTransactionFactorPdfAsync(int transactionId)
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<FactorPdfResult> GenerateListFactorPdfAsync(
        string? direction, int take, bool ascending, IReadOnlyList<int> selectedIds)
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<PartyOptionViewModel>> GetSellersAsync()
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<PartyOptionViewModel>> GetCustomersAsync()
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IReadOnlyList<ProductListItemViewModel>> GetSelectableProductsAsync(TransactionDirection direction)
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<ServiceResult> RecordBuyAsync(MobileShop.Models.ViewModels.Web.BindModels.BuyInputModel input)
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<ServiceResult> RecordSellAsync(MobileShop.Models.ViewModels.Web.BindModels.SellInputModel input)
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<InvoiceViewModel?> GetInvoiceAsync(int transactionId)
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<byte[]> GenerateInvoicePdfAsync(int transactionId)
        => throw new NotImplementedException("ApiTransactionsDataService is not implemented yet.");
}
