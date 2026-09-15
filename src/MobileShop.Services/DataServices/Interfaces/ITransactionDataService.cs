namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for ITransactionDataService.
/// </summary>
public interface ITransactionDataService : IDataService<Transaction>
{
    /// <summary>Gets transactions for a product.</summary>
    /// <param name="productId">The product identifier.</param>
    IEnumerable<Transaction> GetByProduct(int productId);
    /// <summary>Gets transactions for a product.</summary>
    /// <param name="product">The product.</param>
    IEnumerable<Transaction> GetByProduct(Product product);
    /// <summary>Gets transactions for a product asynchronously.</summary>
    /// <param name="productId">The product identifier.</param>
    Task<IEnumerable<Transaction>> GetByProductAsync(int productId);
    /// <summary>Gets transactions for a product asynchronously.</summary>
    /// <param name="product">The product.</param>
    Task<IEnumerable<Transaction>> GetByProductAsync(Product product);

    /// <summary>Gets the most recent transactions.</summary>
    /// <param name="count">The maximum number of transactions.</param>
    IEnumerable<Transaction> GetRecent(int count = 20);
    /// <summary>Gets the most recent transactions asynchronously.</summary>
    /// <param name="count">The maximum number of transactions.</param>
    Task<IEnumerable<Transaction>> GetRecentAsync(int count = 20);
    /// <summary>Gets recent transactions shaped for dashboard cards.</summary>
    /// <param name="count">The maximum number of cards.</param>
    IReadOnlyList<TransactionCardViewModel> GetRecentCards(int count = 20);
    /// <summary>Gets transaction list rows with optional direction and ordering.</summary>
    /// <param name="direction">The optional buy or sell filter.</param>
    /// <param name="take">The maximum number of rows.</param>
    /// <param name="ascending">Whether to order by oldest first.</param>
    IReadOnlyList<TransactionListItemViewModel> GetList(string? direction, int take, bool ascending);
    /// <summary>Gets profit and loss rows within an optional date range.</summary>
    /// <param name="from">The inclusive start date.</param>
    /// <param name="to">The inclusive end date.</param>
    IReadOnlyList<ProfitLossRowViewModel> GetProfitLossRows(DateTime? from, DateTime? to);
    /// <summary>Calculates total profit and loss within an optional date range.</summary>
    /// <param name="from">The inclusive start date.</param>
    /// <param name="to">The inclusive end date.</param>
    decimal GetProfitLossTotal(DateTime? from, DateTime? to);
    /// <summary>Gets transaction rows for a product.</summary>
    /// <param name="productId">The product identifier.</param>
    IReadOnlyList<ProductTransactionViewModel> GetProductTransactions(int productId);
    /// <summary>Gets transaction details, or <see langword="null"/> when not found.</summary>
    /// <param name="id">The transaction identifier.</param>
    TransactionDetailsViewModel? GetDetails(int id);

    // TODO: eager-load Product (+ Phone/AppleId profiles) when reports need full details
    /// <summary>Gets products bought by the shop.</summary>
    /// <param name="predicate">An optional product filter.</param>
    IEnumerable<Product> GetProductsBoughtByShop(Expression<Func<Product, bool>>? predicate = null);
    /// <summary>Gets products sold by the shop.</summary>
    /// <param name="predicate">An optional product filter.</param>
    IEnumerable<Product> GetProductsSoldByShop(Expression<Func<Product, bool>>? predicate = null);
    /// <summary>Gets products bought by the shop asynchronously.</summary>
    /// <param name="predicate">An optional product filter.</param>
    Task<IEnumerable<Product>> GetProductsBoughtByShopAsync(Expression<Func<Product, bool>>? predicate = null);
    /// <summary>Gets products sold by the shop asynchronously.</summary>
    /// <param name="predicate">An optional product filter.</param>
    Task<IEnumerable<Product>> GetProductsSoldByShopAsync(Expression<Func<Product, bool>>? predicate = null);

    /// <summary>Records a buy transaction from a seller.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="finishedPrice">The purchase price.</param>
    /// <param name="date">The optional transaction date.</param>
    bool RecordBuy(int productId, int sellerId, decimal finishedPrice, DateTime? date = null);
    /// <summary>Records a sell transaction to a customer.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="finishedPrice">The sale price.</param>
    /// <param name="date">The optional transaction date.</param>
    bool RecordSell(int productId, int customerId, decimal finishedPrice, DateTime? date = null);
    /// <summary>Records a buy transaction asynchronously.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="finishedPrice">The purchase price.</param>
    /// <param name="date">The optional transaction date.</param>
    Task<bool> RecordBuyAsync(int productId, int sellerId, decimal finishedPrice, DateTime? date = null);
    /// <summary>Records a sell transaction asynchronously.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="customerId">The customer identifier.</param>
    /// <param name="finishedPrice">The sale price.</param>
    /// <param name="date">The optional transaction date.</param>
    Task<bool> RecordSellAsync(int productId, int customerId, decimal finishedPrice, DateTime? date = null);
}
