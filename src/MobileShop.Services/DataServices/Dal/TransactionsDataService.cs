namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the transaction, invoice, and PDF operations for the Transactions area.</summary>
public class TransactionsDataService(
    IBaseRepo<Transaction> transactions,
    IBaseRepo<Seller> sellers,
    IBaseRepo<Customer> customers,
    IBaseRepo<Phone> phones,
    IBaseRepo<AppleId> appleIds,
    IBaseRepo<Product> products,
    AppDbContext context,
    IPdfGenerator pdfGenerator,
    ILogger<TransactionsDataService> logger)
    : ITransactionsDataService
{
    // Shop sentinel records (Person/Seller/Customer Id = 1 in sample data).
    // TODO: move to configuration when the shop entity is configurable.
    private const int ShopSellerId = 1;
    private const int ShopCustomerId = 1;

    /// <summary>Gets the structured logger for this transactions service.</summary>
    protected ILogger<TransactionsDataService> Logger { get; } = logger;

    /// <inheritdoc />
    public async Task<IReadOnlyList<TransactionListItemViewModel>> GetListAsync(
        string? direction,
        int take,
        bool ascending,
        string? sortBy = null)
    {
        var query = await SelectListRowsAsync(direction);

        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();
        var ordered = normalizedSortBy switch
        {
            "product" => ascending
                ? query.OrderBy(transaction => transaction.ProductLabel, StringComparer.OrdinalIgnoreCase)
                : query.OrderByDescending(transaction => transaction.ProductLabel, StringComparer.OrdinalIgnoreCase),
            "price" => ascending
                ? query.OrderBy(transaction => transaction.FinishedPrice)
                : query.OrderByDescending(transaction => transaction.FinishedPrice),
            "seller" => ascending
                ? query.OrderBy(transaction => transaction.SellerLabel, StringComparer.OrdinalIgnoreCase)
                : query.OrderByDescending(transaction => transaction.SellerLabel, StringComparer.OrdinalIgnoreCase),
            "customer" => ascending
                ? query.OrderBy(transaction => transaction.CustomerLabel, StringComparer.OrdinalIgnoreCase)
                : query.OrderByDescending(transaction => transaction.CustomerLabel, StringComparer.OrdinalIgnoreCase),
            _ => ascending
                ? query.OrderBy(transaction => transaction.Date)
                : query.OrderByDescending(transaction => transaction.Date),
        };

        return ordered
            .Take(Math.Clamp(take, 1, 500))
            .ToList();
    }

    /// <summary>Projects the transaction list rows honouring the optional direction filter.</summary>
    private async Task<IEnumerable<TransactionListItemViewModel>> SelectListRowsAsync(string? direction)
    {
        Expression<Func<Transaction, TransactionListItemViewModel>> selector = transaction => new TransactionListItemViewModel(
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
                : transaction.CustomerNavigation.PersonNavigation.FirstName + " " + transaction.CustomerNavigation.PersonNavigation.LastName);

        if (string.Equals(direction, "buy", StringComparison.OrdinalIgnoreCase))
            return await transactions.SelectAllAsync(t => t.Direction == TransactionDirection.Buy, selector);

        if (string.Equals(direction, "sell", StringComparison.OrdinalIgnoreCase))
            return await transactions.SelectAllAsync(t => t.Direction == TransactionDirection.Sell, selector);

        return await transactions.SelectAllAsync(selector);
    }

    /// <inheritdoc />
    public Task<TransactionDetailsViewModel?> GetDetailsAsync(int id)
        => transactions.SelectAsync(
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

    /// <inheritdoc />
    public async Task<byte[]?> GetTransactionFactorPdfAsync(int transactionId)
    {
        var details = await GetDetailsAsync(transactionId);
        if (details is null)
            return null;

        var model = new TransactionFactorViewModel([details.ToFactorRow(transactionId)], DateTime.UtcNow);
        return pdfGenerator.GenerateTransactionFactor(model);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PartyOptionViewModel>> GetSellersAsync()
        => (await sellers
            .SelectAllAsync(seller => new PartyOptionViewModel(
                seller.Id,
                seller.PersonNavigation.FirstName + " " + seller.PersonNavigation.LastName,
                seller.EntityType.ToString())))
            .OrderBy(row => row.Label)
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<PartyOptionViewModel>> GetCustomersAsync()
        => (await customers
            .SelectAllAsync(customer => new PartyOptionViewModel(
                customer.Id,
                customer.PersonNavigation.FirstName + " " + customer.PersonNavigation.LastName,
                customer.PersonNavigation.PhoneNumber)))
            .OrderBy(row => row.Label)
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> GetSelectableProductsAsync(TransactionDirection direction)
    {
        var phoneRows = await phones.SelectAllAsync(
            phone => phone.ProductNavigation.Transactions.All(transaction => transaction.Direction != direction),
            phone => new ProductListItemViewModel(
                phone.Id,
                phone.ProductId,
                "Phone",
                phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                "IMEI: " + phone.IMEI1,
                (phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name),
                phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                phone.ProductNavigation.SecondHandProfile != null)
            {
                PartNumberLabel = phone.PartNumberNavigation == null ? "N/A" : phone.PartNumberNavigation.Code,
                SuggestedPrice = phone.ProductNavigation.Price,
            });

        var glassRows = await products.SelectAllAsync(
            product => product.GlassProfile != null && product.Transactions.All(transaction => transaction.Direction != direction),
            product => new ProductListItemViewModel(
                product.Id,
                product.Id,
                "Glass",
                product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name,
                "Barcode: " + product.Barcode,
                null,
                product.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                product.SecondHandProfile != null)
            {
                SuggestedPrice = product.Price,
            });

        var appleIdRows = await appleIds.SelectAllAsync(
            appleId => appleId.ProductNavigation.Transactions.All(transaction => transaction.Direction != direction),
            appleId => new ProductListItemViewModel(
                appleId.Id,
                appleId.ProductId,
                "Apple ID",
                appleId.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + appleId.ProductNavigation.ModelNavigation.Name,
                appleId.Email,
                null,
                appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                appleId.ProductNavigation.SecondHandProfile != null)
            {
                SuggestedPrice = appleId.ProductNavigation.Price,
            });

        return phoneRows
            .Concat(glassRows)
            .Concat(appleIdRows)
            .OrderBy(row => row.Name)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> SearchSelectableProductsAsync(
        TransactionDirection direction,
        string? q,
        int take = 25)
    {
        var normalizedQuery = q?.Trim();
        var limit = Math.Clamp(take, 1, 500);
        var selectable = await GetSelectableProductsAsync(direction);

        if (string.IsNullOrEmpty(normalizedQuery))
            return selectable.Take(limit).ToList();

        return selectable
            .Where(row =>
                Contains(row.Name, normalizedQuery) ||
                Contains(row.Type, normalizedQuery) ||
                Contains(row.Identifier, normalizedQuery) ||
                Contains(row.Color, normalizedQuery) ||
                Contains(row.PartNumberLabel, normalizedQuery))
            .Take(limit)
            .ToList();
    }

    private static bool Contains(string? value, string query) =>
        !string.IsNullOrEmpty(value) &&
        value.Contains(query, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<ServiceResult> RecordBuyAsync(BuyInputModel input)
    {
        if (input.Price < 0 || input.Price > MoneyLimits.MaxRials)
        {
            Logger.LogWarning("RecordBuy rejected: invalid price {Price}", input.Price);
            return new ServiceResult(false, input.Price > MoneyLimits.MaxRials ? "The price is too large." : "The buy could not be recorded. Check the product and price.", nameof(BuyInputModel.Price), null);
        }

        var existingBuy = (await transactions.FindAllAsync(transaction => transaction.ProductId == input.ProductId))
            .Any(transaction => transaction.Direction == TransactionDirection.Buy);

        if (existingBuy)
        {
            Logger.LogWarning(
                "RecordBuy rejected: product Id={ProductId} already has a Buy transaction",
                input.ProductId);
            return new ServiceResult(false, "The buy could not be recorded. Check the product and price.", null, null);
        }

        var transaction = new Transaction
        {
            ProductId = input.ProductId,
            SellerId = input.SellerId,
            CustomerId = ShopCustomerId,
            FinishedPrice = input.Price,
            Date = input.Date ?? DateTime.Today,
            Direction = TransactionDirection.Buy
        };

        if (await transactions.AddAsync(transaction) <= 0)
        {
            Logger.LogWarning("Failed to record Buy: ProductId={ProductId}", input.ProductId);
            return new ServiceResult(false, "The buy could not be recorded. Check the product and price.", null, null);
        }

        Logger.LogInformation(
            "Recorded Buy: ProductId={ProductId}, SellerId={SellerId}, Price={Price}",
            input.ProductId, input.SellerId, input.Price);
        return new ServiceResult(true, null, null, transaction.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> RecordSellAsync(SellInputModel input)
    {
        if (input.Price < 0 || input.Price > MoneyLimits.MaxRials)
        {
            Logger.LogWarning("RecordSell rejected: invalid price {Price}", input.Price);
            return new ServiceResult(false, input.Price > MoneyLimits.MaxRials ? "The price is too large." : "The sale could not be recorded. Check the product and price.", nameof(SellInputModel.Price), null);
        }

        var alreadySold = (await transactions.FindAllAsync(transaction => transaction.ProductId == input.ProductId))
            .Any(transaction => transaction.Direction == TransactionDirection.Sell);

        if (alreadySold)
        {
            Logger.LogWarning(
                "RecordSell rejected: product Id={ProductId} already sold",
                input.ProductId);
            return new ServiceResult(false, "The sale could not be recorded. Check the product and price.", null, null);
        }

        var transaction = new Transaction
        {
            ProductId = input.ProductId,
            SellerId = ShopSellerId,
            CustomerId = input.CustomerId,
            FinishedPrice = input.Price,
            Date = input.Date ?? DateTime.Today,
            Direction = TransactionDirection.Sell
        };

        if (await transactions.AddAsync(transaction) <= 0)
        {
            Logger.LogWarning("Failed to record Sell: ProductId={ProductId}", input.ProductId);
            return new ServiceResult(false, "The sale could not be recorded. Check the product and price.", null, null);
        }

        Logger.LogInformation(
            "Recorded Sell: ProductId={ProductId}, CustomerId={CustomerId}, Price={Price}",
            input.ProductId, input.CustomerId, input.Price);
        return new ServiceResult(true, null, null, transaction.Id);
    }

    /// <inheritdoc />
    public Task<InvoiceViewModel?> GetInvoiceAsync(int transactionId)
        => Task.FromResult(BuildInvoice(transactionId));

    /// <inheritdoc />
    public Task<byte[]> GenerateInvoicePdfAsync(int transactionId)
        => Task.FromResult(pdfGenerator.Generate(BuildInvoice(transactionId)
            ?? throw new KeyNotFoundException($"Transaction {transactionId} was not found.")));

    /// <summary>Assembles the invoice model for a transaction, or <c>null</c> when it does not exist.</summary>
    private InvoiceViewModel? BuildInvoice(int transactionId)
    {
        var transaction = LoadInvoiceGraph(transactionId);
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

    /// <inheritdoc />
    public async Task<FactorPdfResult> GenerateListFactorPdfAsync(
        string? direction,
        int take,
        bool ascending,
        IReadOnlyList<int> selectedIds,
        string? sortBy = null)
    {
        // The factor always works from one list snapshot - the same read the list page shows.
        var snapshot = await GetListAsync(direction, take, ascending, sortBy);

        IReadOnlyList<TransactionFactorRowViewModel> rows;

        if (selectedIds is null || selectedIds.Count == 0)
        {
            if (snapshot.Count == 0)
            {
                Logger.LogWarning("GenerateListFactorPdfAsync rejected: no transactions match the current filters");
                return new FactorPdfResult(false, null, "No transactions match the current filters.");
            }

            rows = snapshot.Select(transaction => transaction.ToFactorRow()).ToList();
        }
        else
        {
            if (selectedIds.Any(id => id <= 0))
            {
                Logger.LogWarning("GenerateListFactorPdfAsync rejected: non-positive selected identifier");
                return new FactorPdfResult(false, null, "Selected transaction identifiers must be positive numbers.");
            }

            var requested = selectedIds.Distinct().ToList();
            var lookup = snapshot.ToDictionary(transaction => transaction.Id);

            var resolved = new List<TransactionFactorRowViewModel>(requested.Count);
            var missing = new List<int>();
            foreach (var id in requested)
            {
                if (lookup.TryGetValue(id, out var transaction))
                {
                    resolved.Add(transaction.ToFactorRow());
                    continue;
                }

                // Identifiers absent from the snapshot are reported rather than fetched individually,
                // preserving the single-snapshot guarantee and never producing a partial factor.
                missing.Add(id);
            }

            if (missing.Count > 0)
            {
                Logger.LogWarning(
                    "GenerateListFactorPdfAsync rejected: missing transaction ids {Missing}",
                    string.Join(",", missing));
                return new FactorPdfResult(
                    false,
                    null,
                    $"These selected transactions no longer exist: {string.Join(", ", missing)}.");
            }

            rows = resolved;
        }

        var model = new TransactionFactorViewModel(rows, DateTime.UtcNow);
        return new FactorPdfResult(true, pdfGenerator.GenerateTransactionFactor(model), null);
    }

    /// <summary>
    /// Loads the invoice navigation graph for a transaction. Uses <see cref="AppDbContext"/> directly
    /// because the invoice shape needs eager <c>Include</c> chains that <see cref="IBaseRepo{T}"/> does not expose.
    /// </summary>
    private Transaction? LoadInvoiceGraph(int transactionId)
        => context.Transactions
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
                .ThenInclude(s => s!.StorageCapacityNavigation)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.CaseProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.GlassProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.TabletProfile)
            .Include(t => t.ProductNavigation).ThenInclude(p => p.SmartWatchProfile)
            .FirstOrDefault(t => t.Id == transactionId);

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
