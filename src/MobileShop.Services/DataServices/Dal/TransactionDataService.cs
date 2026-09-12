namespace MobileShop.Services.DataServices.Dal;

public class TransactionDataService(
    ITransactionRepo transactionRepo,
    ILogger<TransactionDataService> logger)
    : DataServiceBase<TransactionDataService, Transaction>(transactionRepo, logger),
      ITransactionDataService
{
    private readonly ITransactionRepo _transactionRepo = transactionRepo;

    // Shop sentinel records (Person/Seller/Customer Id = 1 in sample data).
    // TODO: move to configuration when the shop entity is configurable.
    private const int ShopSellerId = 1;
    private const int ShopCustomerId = 1;

    // ── By product ────────────────────────────────────────

    public IEnumerable<Transaction> GetByProduct(int productId)
        => _transactionRepo.GetByProduct(productId);

    public IEnumerable<Transaction> GetByProduct(Product product)
        => GetByProduct(product.Id);

    public Task<IEnumerable<Transaction>> GetByProductAsync(int productId)
        => _transactionRepo.GetByProductAsync(productId);

    public Task<IEnumerable<Transaction>> GetByProductAsync(Product product)
        => GetByProductAsync(product.Id);

    // ── Recent ────────────────────────────────────────────

    public IEnumerable<Transaction> GetRecent(int count = 20)
        => _transactionRepo
            .GetAll()
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToList();

    public async Task<IEnumerable<Transaction>> GetRecentAsync(int count = 20)
        => (await _transactionRepo.GetAllAsync())
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToList();

    // ── Products bought / sold by the shop ────────────────

    public IEnumerable<Product> GetProductsBoughtByShop(
        Expression<Func<Product, bool>>? predicate = null)
    {
        var products = _transactionRepo
            .GetAll(t => t.Direction == TransactionDirection.Buy)
            .Select(t => t.ProductNavigation)
            .Where(p => p is not null)
            .Distinct()
            .AsQueryable();

        if (predicate is not null)
            products = products.Where(predicate);

        return products.ToList();
    }

    public IEnumerable<Product> GetProductsSoldByShop(
        Expression<Func<Product, bool>>? predicate = null)
    {
        var products = _transactionRepo
            .GetAll(t => t.Direction == TransactionDirection.Sell)
            .Select(t => t.ProductNavigation)
            .Where(p => p is not null)
            .Distinct()
            .AsQueryable();

        if (predicate is not null)
            products = products.Where(predicate);

        return products.ToList();
    }

    public async Task<IEnumerable<Product>> GetProductsBoughtByShopAsync(
        Expression<Func<Product, bool>>? predicate = null)
    {
        var products = (await _transactionRepo
                .GetAllAsync(t => t.Direction == TransactionDirection.Buy))
            .Select(t => t.ProductNavigation)
            .Where(p => p is not null)
            .Distinct()
            .AsQueryable();

        if (predicate is not null)
            products = products.Where(predicate);

        return products.ToList();
    }

    public async Task<IEnumerable<Product>> GetProductsSoldByShopAsync(
        Expression<Func<Product, bool>>? predicate = null)
    {
        var products = (await _transactionRepo
                .GetAllAsync(t => t.Direction == TransactionDirection.Sell))
            .Select(t => t.ProductNavigation)
            .Where(p => p is not null)
            .Distinct()
            .AsQueryable();

        if (predicate is not null)
            products = products.Where(predicate);

        return products.ToList();
    }

    // ── Record buy / sell ─────────────────────────────────

    public bool RecordBuy(
        int productId,
        int sellerId,
        decimal finishedPrice,
        DateTime? date = null)
    {
        if (finishedPrice < 0)
        {
            Logger.LogWarning("RecordBuy rejected: negative price {Price}", finishedPrice);
            return false;
        }

        // Already sold? Still allow buy only if no prior buy — keep simple for now.
        var existingBuy = _transactionRepo
            .GetByProduct(productId)
            .Any(t => t.Direction == TransactionDirection.Buy);

        if (existingBuy)
        {
            Logger.LogWarning(
                "RecordBuy rejected: product Id={ProductId} already has a Buy transaction",
                productId);
            return false;
        }

        var transaction = new Transaction
        {
            ProductId = productId,
            SellerId = sellerId,
            CustomerId = ShopCustomerId,
            FinishedPrice = finishedPrice,
            Date = date ?? DateTime.UtcNow,
            Direction = TransactionDirection.Buy
        };

        var ok = Add(transaction);
        if (ok)
            Logger.LogInformation(
                "Recorded Buy: ProductId={ProductId}, SellerId={SellerId}, Price={Price}",
                productId, sellerId, finishedPrice);

        return ok;
    }

    public bool RecordSell(
        int productId,
        int customerId,
        decimal finishedPrice,
        DateTime? date = null)
    {
        if (finishedPrice < 0)
        {
            Logger.LogWarning("RecordSell rejected: negative price {Price}", finishedPrice);
            return false;
        }

        var alreadySold = _transactionRepo
            .GetByProduct(productId)
            .Any(t => t.Direction == TransactionDirection.Sell);

        if (alreadySold)
        {
            Logger.LogWarning(
                "RecordSell rejected: product Id={ProductId} already sold",
                productId);
            return false;
        }

        var transaction = new Transaction
        {
            ProductId = productId,
            SellerId = ShopSellerId,
            CustomerId = customerId,
            FinishedPrice = finishedPrice,
            Date = date ?? DateTime.UtcNow,
            Direction = TransactionDirection.Sell
        };

        var ok = Add(transaction);
        if (ok)
            Logger.LogInformation(
                "Recorded Sell: ProductId={ProductId}, CustomerId={CustomerId}, Price={Price}",
                productId, customerId, finishedPrice);

        return ok;
    }

    public Task<bool> RecordBuyAsync(
        int productId,
        int sellerId,
        decimal finishedPrice,
        DateTime? date = null)
    {
        // Keep logic in one place — sync path is fine for now; can fully async later.
        return Task.FromResult(RecordBuy(productId, sellerId, finishedPrice, date));
    }

    public Task<bool> RecordSellAsync(
        int productId,
        int customerId,
        decimal finishedPrice,
        DateTime? date = null)
        => Task.FromResult(RecordSell(productId, customerId, finishedPrice, date));
}