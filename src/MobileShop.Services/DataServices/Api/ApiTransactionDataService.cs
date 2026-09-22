namespace MobileShop.Services.DataServices.Api;

public class ApiTransactionDataService : ITransactionDataService
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
    public IEnumerable<Transaction> FindAll(Expression<Func<Transaction, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<Transaction>> FindAllAsync(Expression<Func<Transaction, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Transaction? Find(int id)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Transaction? Find(Expression<Func<Transaction, bool>> predicate)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Transaction?> FindAsync(int id)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<Transaction?> FindAsync(Expression<Func<Transaction, bool>> predicate)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Add(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Update(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(int id)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> AddAsync(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> UpdateAsync(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");
}
