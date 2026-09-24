namespace MobileShop.Services.DataServices.Api;

public class ApiTransactionDataService : ApiDataServiceBase<Transaction>, ITransactionDataService
{
    /// <inheritdoc />
    public IEnumerable<Transaction> GetByProduct(int productId)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Transaction> GetByProduct(Product product)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Transaction>> GetByProductAsync(int productId)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Transaction>> GetByProductAsync(Product product)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Transaction> GetRecent(int count = 20)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Transaction>> GetRecentAsync(int count = 20)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<TransactionCardViewModel> GetRecentCards(int count = 20)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<TransactionListItemViewModel> GetList(string? direction, int take, bool ascending)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<ProfitLossRowViewModel> GetProfitLossRows(DateTime? from, DateTime? to)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public decimal GetProfitLossTotal(DateTime? from, DateTime? to)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public IReadOnlyList<ProductTransactionViewModel> GetProductTransactions(int productId)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public TransactionDetailsViewModel? GetDetails(int id)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Product> GetProductsBoughtByShop(Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public IEnumerable<Product> GetProductsSoldByShop(Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Product>> GetProductsBoughtByShopAsync(Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Product>> GetProductsSoldByShopAsync(Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public bool RecordBuy(int productId, int sellerId, decimal finishedPrice, DateTime? date = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public bool RecordSell(int productId, int customerId, decimal finishedPrice, DateTime? date = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> RecordBuyAsync(int productId, int sellerId, decimal finishedPrice, DateTime? date = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> RecordSellAsync(int productId, int customerId, decimal finishedPrice, DateTime? date = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public byte[] GenerateTransactionsPdf(string? direction, int take, string order)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");
}
