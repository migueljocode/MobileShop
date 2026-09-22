namespace MobileShop.Services.DataServices.Api;

public class ApiTransactionDataService : ITransactionDataService
{
    public IEnumerable<Transaction> GetByProduct(int productId)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public IEnumerable<Transaction> GetByProduct(Product product)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<IEnumerable<Transaction>> GetByProductAsync(int productId)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<IEnumerable<Transaction>> GetByProductAsync(Product product)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public IEnumerable<Transaction> GetRecent(int count = 20)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<IEnumerable<Transaction>> GetRecentAsync(int count = 20)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public IReadOnlyList<TransactionCardViewModel> GetRecentCards(int count = 20)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public IReadOnlyList<TransactionListItemViewModel> GetList(string? direction, int take, bool ascending)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public IReadOnlyList<ProfitLossRowViewModel> GetProfitLossRows(DateTime? from, DateTime? to)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public decimal GetProfitLossTotal(DateTime? from, DateTime? to)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public IReadOnlyList<ProductTransactionViewModel> GetProductTransactions(int productId)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public TransactionDetailsViewModel? GetDetails(int id)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public IEnumerable<Product> GetProductsBoughtByShop(System.Linq.Expressions.Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public IEnumerable<Product> GetProductsSoldByShop(System.Linq.Expressions.Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<IEnumerable<Product>> GetProductsBoughtByShopAsync(System.Linq.Expressions.Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<IEnumerable<Product>> GetProductsSoldByShopAsync(System.Linq.Expressions.Expression<Func<Product, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public bool RecordBuy(int productId, int sellerId, decimal finishedPrice, DateTime? date = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public bool RecordSell(int productId, int customerId, decimal finishedPrice, DateTime? date = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<bool> RecordBuyAsync(int productId, int sellerId, decimal finishedPrice, DateTime? date = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<bool> RecordSellAsync(int productId, int customerId, decimal finishedPrice, DateTime? date = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public IEnumerable<Transaction> FindAll(System.Linq.Expressions.Expression<Func<Transaction, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<IEnumerable<Transaction>> FindAllAsync(System.Linq.Expressions.Expression<Func<Transaction, bool>>? predicate = null)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Transaction? Find(int id)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Transaction? Find(System.Linq.Expressions.Expression<Func<Transaction, bool>> predicate)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<Transaction?> FindAsync(int id)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<Transaction?> FindAsync(System.Linq.Expressions.Expression<Func<Transaction, bool>> predicate)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public bool Add(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public bool Update(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public bool Delete(int id)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public bool Delete(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<bool> AddAsync(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<bool> UpdateAsync(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");

    public Task<bool> DeleteAsync(Transaction entity)
        => throw new NotImplementedException("ApiTransactionDataService is not implemented yet.");
}
