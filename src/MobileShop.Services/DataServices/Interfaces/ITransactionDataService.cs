namespace MobileShop.Services.DataServices.Interfaces;

public interface ITransactionDataService : IDataService<Transaction>
{
    IEnumerable<Transaction> GetByProduct(int productId);
    IEnumerable<Transaction> GetByProduct(Product product);
    Task<IEnumerable<Transaction>> GetByProductAsync(int productId);
    Task<IEnumerable<Transaction>> GetByProductAsync(Product product);

    IEnumerable<Transaction> GetRecent(int count = 20);
    Task<IEnumerable<Transaction>> GetRecentAsync(int count = 20);
    IReadOnlyList<TransactionCardViewModel> GetRecentCards(int count = 20);
    IReadOnlyList<TransactionListItemViewModel> GetList(string? direction, int take, bool ascending);
    IReadOnlyList<ProfitLossRowViewModel> GetProfitLossRows(DateTime? from, DateTime? to);
    decimal GetProfitLossTotal(DateTime? from, DateTime? to);
    IReadOnlyList<ProductTransactionViewModel> GetProductTransactions(int productId);

    // TODO: eager-load Product (+ Phone/AppleId profiles) when reports need full details
    IEnumerable<Product> GetProductsBoughtByShop(Expression<Func<Product, bool>>? predicate = null);
    IEnumerable<Product> GetProductsSoldByShop(Expression<Func<Product, bool>>? predicate = null);
    Task<IEnumerable<Product>> GetProductsBoughtByShopAsync(Expression<Func<Product, bool>>? predicate = null);
    Task<IEnumerable<Product>> GetProductsSoldByShopAsync(Expression<Func<Product, bool>>? predicate = null);

    bool RecordBuy(int productId, int sellerId, decimal finishedPrice, DateTime? date = null);
    bool RecordSell(int productId, int customerId, decimal finishedPrice, DateTime? date = null);
    Task<bool> RecordBuyAsync(int productId, int sellerId, decimal finishedPrice, DateTime? date = null);
    Task<bool> RecordSellAsync(int productId, int customerId, decimal finishedPrice, DateTime? date = null);
}