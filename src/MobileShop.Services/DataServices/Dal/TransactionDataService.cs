namespace MobileShop.Services.DataServices.Dal;

public class TransactionDataService(
    ITransactionRepo transactionRepo,
    ILogger<TransactionDataService> logger,
    IPdfGenerator pdfGenerator)
    : DataServiceBase<TransactionDataService, Transaction>(transactionRepo, logger),
      ITransactionDataService
{
    private readonly ITransactionRepo _transactionRepo = transactionRepo;
    private readonly IPdfGenerator _pdfGenerator = pdfGenerator;

    // Shop sentinel records (Person/Seller/Customer Id = 1 in sample data).
    // TODO: move to configuration when the shop entity is configurable.
    private const int ShopSellerId = 1;
    private const int ShopCustomerId = 1;

    // ── By product ────────────────────────────────────────

    /// <inheritdoc />
    public IEnumerable<Transaction> GetByProduct(int productId)
        => _transactionRepo.GetByProduct(productId);

    /// <inheritdoc />
    public IEnumerable<Transaction> GetByProduct(Product product)
        => GetByProduct(product.Id);

    /// <inheritdoc />
    public Task<IEnumerable<Transaction>> GetByProductAsync(int productId)
        => _transactionRepo.GetByProductAsync(productId);

    /// <inheritdoc />
    public Task<IEnumerable<Transaction>> GetByProductAsync(Product product)
        => GetByProductAsync(product.Id);

    // ── Recent ────────────────────────────────────────────

    /// <inheritdoc />
    public IEnumerable<Transaction> GetRecent(int count = 20)
        => _transactionRepo
            .FindAll()
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToList();

    /// <inheritdoc />
    public async Task<IEnumerable<Transaction>> GetRecentAsync(int count = 20)
        => (await _transactionRepo.FindAllAsync())
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToList();

    /// <inheritdoc />
    public IReadOnlyList<TransactionCardViewModel> GetRecentCards(int count = 20)
        => _transactionRepo
            .SelectAll(transaction => new TransactionCardViewModel(
                transaction.Date,
                transaction.Direction,
                transaction.FinishedPrice,
                transaction.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + transaction.ProductNavigation.ModelNavigation.Name,
                transaction.Direction == TransactionDirection.Buy
                    ? "From: " + (transaction.SellerNavigation.PersonNavigation == null
                        ? "Shop"
                        : transaction.SellerNavigation.PersonNavigation.FirstName + " " + transaction.SellerNavigation.PersonNavigation.LastName)
                    : "To: " + (transaction.CustomerNavigation.PersonNavigation == null
                        ? "Shop"
                        : transaction.CustomerNavigation.PersonNavigation.FirstName + " " + transaction.CustomerNavigation.PersonNavigation.LastName)))
            .OrderByDescending(card => card.Date)
            .Take(count)
            .ToList();

    /// <inheritdoc />
    public IReadOnlyList<TransactionListItemViewModel> GetList(
        string? direction,
        int take,
        bool ascending)
    {
        var query = _transactionRepo.SelectAll(transaction => new TransactionListItemViewModel(
                transaction.Id,
                transaction.Date,
                transaction.Direction,
                transaction.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + transaction.ProductNavigation.ModelNavigation.Name,
                transaction.FinishedPrice,
                transaction.SellerNavigation.PersonNavigation == null
                    ? "Shop"
                    : transaction.SellerNavigation.PersonNavigation.FirstName + " " + transaction.SellerNavigation.PersonNavigation.LastName,
                transaction.CustomerNavigation.PersonNavigation == null
                    ? "Shop"
                    : transaction.CustomerNavigation.PersonNavigation.FirstName + " " + transaction.CustomerNavigation.PersonNavigation.LastName));

        if (string.Equals(direction, "buy", StringComparison.OrdinalIgnoreCase))
            query = query.Where(t => t.Direction == TransactionDirection.Buy).ToList();
        else if (string.Equals(direction, "sell", StringComparison.OrdinalIgnoreCase))
            query = query.Where(t => t.Direction == TransactionDirection.Sell).ToList();

        var ordered = ascending
            ? query.OrderBy(t => t.Date)
            : query.OrderByDescending(t => t.Date);

        return ordered
            .Take(Math.Clamp(take, 1, 500))
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<ProfitLossRowViewModel> GetProfitLossRows(DateTime? from, DateTime? to)
    {
        // Projected so the product navigation is read inside the query instead of relying on
        // eager-loaded entities.
        var transactions = _transactionRepo
            .SelectAll(transaction => new
            {
                transaction.ProductId,
                transaction.Date,
                transaction.Direction,
                transaction.FinishedPrice,
                ProductLabel = transaction.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + transaction.ProductNavigation.ModelNavigation.Name
            })
            .Where(transaction => (!from.HasValue || transaction.Date.Date >= from.Value.Date) &&
                                  (!to.HasValue || transaction.Date.Date <= to.Value.Date));

        return transactions
            .GroupBy(transaction => transaction.ProductId)
            .Select(group =>
            {
                var first = group.First();
                return new ProfitLossRowViewModel(
                    group.Key,
                    first.ProductLabel,
                    group.Where(t => t.Direction == TransactionDirection.Buy).Sum(t => t.FinishedPrice),
                    group.Where(t => t.Direction == TransactionDirection.Sell).Sum(t => t.FinishedPrice));
            })
            .OrderBy(row => row.ProductId)
            .ToList();
    }

    /// <inheritdoc />
    public decimal GetProfitLossTotal(DateTime? from, DateTime? to)
        => GetProfitLossRows(from, to).Sum(row => row.Profit);

    /// <inheritdoc />
    public IReadOnlyList<ProductTransactionViewModel> GetProductTransactions(int productId)
        => _transactionRepo
            .SelectAll(
                transaction => transaction.ProductId == productId,
                transaction => new ProductTransactionViewModel(
                    transaction.Date,
                    transaction.Direction,
                    transaction.FinishedPrice,
                    transaction.SellerNavigation.PersonNavigation == null
                        ? "Shop"
                        : transaction.SellerNavigation.PersonNavigation.FirstName + " " + transaction.SellerNavigation.PersonNavigation.LastName,
                    transaction.CustomerNavigation.PersonNavigation == null
                        ? "Shop"
                        : transaction.CustomerNavigation.PersonNavigation.FirstName + " " + transaction.CustomerNavigation.PersonNavigation.LastName))
            .OrderByDescending(item => item.Date)
            .ToList();

    /// <inheritdoc />
    public TransactionDetailsViewModel? GetDetails(int id)
        => _transactionRepo.Select(
            id,
            transaction => new TransactionDetailsViewModel(
                transaction.Date,
                transaction.Direction,
                transaction.FinishedPrice,
                transaction.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + transaction.ProductNavigation.ModelNavigation.Name,
                transaction.SellerNavigation.PersonNavigation == null
                    ? "Shop"
                    : transaction.SellerNavigation.PersonNavigation.FirstName + " " + transaction.SellerNavigation.PersonNavigation.LastName,
                transaction.CustomerNavigation.PersonNavigation == null
                    ? "Shop"
                    : transaction.CustomerNavigation.PersonNavigation.FirstName + " " + transaction.CustomerNavigation.PersonNavigation.LastName));

    // ── Products bought / sold by the shop ────────────────

    /// <inheritdoc />
    public IEnumerable<Product> GetProductsBoughtByShop(
        Expression<Func<Product, bool>>? predicate = null)
    {
        var products = _transactionRepo
            .SelectAll(
                transaction => transaction.Direction == TransactionDirection.Buy,
                transaction => transaction.ProductNavigation)
            .Where(product => product is not null)
            .Distinct()
            .AsQueryable();

        if (predicate is not null)
            products = products.Where(predicate);

        return products.ToList();
    }

    /// <inheritdoc />
    public IEnumerable<Product> GetProductsSoldByShop(
        Expression<Func<Product, bool>>? predicate = null)
    {
        var products = _transactionRepo
            .SelectAll(
                transaction => transaction.Direction == TransactionDirection.Sell,
                transaction => transaction.ProductNavigation)
            .Where(product => product is not null)
            .Distinct()
            .AsQueryable();

        if (predicate is not null)
            products = products.Where(predicate);

        return products.ToList();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetProductsBoughtByShopAsync(
        Expression<Func<Product, bool>>? predicate = null)
    {
        var products = (await _transactionRepo
                .SelectAllAsync(
                    transaction => transaction.Direction == TransactionDirection.Buy,
                    transaction => transaction.ProductNavigation))
            .Where(product => product is not null)
            .Distinct()
            .AsQueryable();

        if (predicate is not null)
            products = products.Where(predicate);

        return products.ToList();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetProductsSoldByShopAsync(
        Expression<Func<Product, bool>>? predicate = null)
    {
        var products = (await _transactionRepo
                .SelectAllAsync(
                    transaction => transaction.Direction == TransactionDirection.Sell,
                    transaction => transaction.ProductNavigation))
            .Where(product => product is not null)
            .Distinct()
            .AsQueryable();

        if (predicate is not null)
            products = products.Where(predicate);

        return products.ToList();
    }

    // ── Record buy / sell ─────────────────────────────────

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Task<bool> RecordBuyAsync(
        int productId,
        int sellerId,
        decimal finishedPrice,
        DateTime? date = null)
    {
        // Keep logic in one place — sync path is fine for now; can fully async later.
        return Task.FromResult(RecordBuy(productId, sellerId, finishedPrice, date));
    }

    /// <inheritdoc />
    public Task<bool> RecordSellAsync(
        int productId,
        int customerId,
        decimal finishedPrice,
        DateTime? date = null)
        => Task.FromResult(RecordSell(productId, customerId, finishedPrice, date));

    private static string SellerLabel(Transaction transaction)
        => transaction.SellerId == ShopSellerId
            ? "Shop"
            : transaction.SellerNavigation?.PersonNavigation is { } person
                ? $"{person.FirstName} {person.LastName}"
                : "Seller";

    private static string CustomerLabel(Transaction transaction)
        => transaction.CustomerId == ShopCustomerId
            ? "Shop"
            : transaction.CustomerNavigation?.PersonNavigation is { } person
                ? $"{person.FirstName} {person.LastName}"
                : "Customer";

    /// <inheritdoc />
    public byte[] GenerateTransactionsPdf(string? direction, int take, string order)
    {
        var kind = direction == "buy" ? "Purchase List" : direction == "sell" ? "Sales List" : "Transactions List";
        var info = $"Transactions ({(direction ?? "all")}, {take}, {order})";
        var notes = $"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";

        var model = new InvoiceViewModel(
            BuyerName: null,
            BuyerNationalId: null,
            BuyerPhoneNumber: null,
            SellerName: null,
            SellerPhoneNumber: null,
            TransactionDate: DateTime.UtcNow,
            FinishedPrice: 0,
            ProductCount: take,
            ProductInformation: info,
            ProductExtras: [],
            GuaranteeInformation: [],
            OwnershipTransferred: null,
            Notes: notes
        );

        return _pdfGenerator.Generate(model);
    }
}