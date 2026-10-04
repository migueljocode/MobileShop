namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the customer and seller operations for the People area.</summary>
public class PeopleDataService(
    IBaseRepo<Customer> customers,
    IBaseRepo<Seller> sellers,
    IBaseRepo<Product> products,
    ILogger<PeopleDataService> logger)
    : IPeopleDataService
{
    /// <summary>Gets the structured logger for this people service.</summary>
    protected ILogger<PeopleDataService> Logger { get; } = logger;

    /// <inheritdoc />
    public async Task<IReadOnlyList<CustomerListItemViewModel>> GetCustomerRowsAsync(string sortBy, bool ascending)
    {
        var rows = await customers.SelectAllAsync(customer => new CustomerListItemViewModel(
            customer.Id,
            customer.PersonNavigation.FirstName + " " + customer.PersonNavigation.LastName,
            customer.PersonNavigation.PhoneNumber,
            customer.NationalId,
            customer.Transactions.Count(t => t.Direction == TransactionDirection.Sell && !t.IsDeleted)));

        return (sortBy, ascending) switch
        {
            ("Phone", true) => rows.OrderBy(row => row.PhoneNumber).ToList(),
            ("Phone", false) => rows.OrderByDescending(row => row.PhoneNumber).ToList(),
            ("Count", true) => rows.OrderBy(row => row.PurchasedCount).ToList(),
            ("Count", false) => rows.OrderByDescending(row => row.PurchasedCount).ToList(),
            (_, false) => rows.OrderByDescending(row => row.Name).ToList(),
            _ => rows.OrderBy(row => row.Name).ToList(),
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SellerListItemViewModel>> GetSellerRowsAsync(string sortBy, bool ascending)
    {
        var rows = await sellers.SelectAllAsync(seller => new SellerListItemViewModel(
            seller.Id,
            seller.PersonNavigation.FirstName + " " + seller.PersonNavigation.LastName,
            seller.PersonNavigation.PhoneNumber,
            seller.EntityType.ToString(),
            seller.Transactions.Count(t => t.Direction == TransactionDirection.Buy && !t.IsDeleted)));

        return (sortBy, ascending) switch
        {
            ("Phone", true) => rows.OrderBy(row => row.PhoneNumber).ToList(),
            ("Phone", false) => rows.OrderByDescending(row => row.PhoneNumber).ToList(),
            ("Count", true) => rows.OrderBy(row => row.SoldCount).ToList(),
            ("Count", false) => rows.OrderByDescending(row => row.SoldCount).ToList(),
            (_, false) => rows.OrderByDescending(row => row.Name).ToList(),
            _ => rows.OrderBy(row => row.Name).ToList(),
        };
    }

    /// <inheritdoc />
    public async Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id)
    {
        var details = await customers.SelectAsync(
            id,
            customer => new CustomerDetailsViewModel(
                customer.PersonNavigation.FirstName + " " + customer.PersonNavigation.LastName,
                customer.PersonNavigation.PhoneNumber,
                customer.NationalId));
        if (details is null)
            return null;

        // Products purchased from the shop: Sell-direction transactions for this customer.
        var purchasedIds = (await customers.SelectAllAsync(
                customer => customer.Id == id,
                customer => customer.Transactions
                    .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                    .Select(transaction => transaction.ProductId)
                    .ToList()))
            .SelectMany(ids => ids)
            .Distinct()
            .ToHashSet();

        return details with { Products = await GetInventoryRowsForAsync(purchasedIds) };
    }

    /// <inheritdoc />
    public async Task<SellerDetailsViewModel?> GetSellerDetailsAsync(int id)
    {
        var details = await sellers.SelectAsync(
            id,
            seller => new SellerDetailsViewModel(
                seller.PersonNavigation.FirstName + " " + seller.PersonNavigation.LastName,
                seller.PersonNavigation.PhoneNumber,
                seller.EntityType.ToString()));
        if (details is null)
            return null;

        // Products supplied to the shop: Buy-direction transactions for this seller only.
        var suppliedIds = (await sellers.SelectAllAsync(
                seller => seller.Id == id,
                seller => seller.Transactions
                    .Where(transaction => transaction.Direction == TransactionDirection.Buy)
                    .Select(transaction => transaction.ProductId)
                    .ToList()))
            .SelectMany(ids => ids)
            .Distinct()
            .ToHashSet();

        return details with { Products = await GetInventoryRowsForAsync(suppliedIds) };
    }

    /// <summary>Projects inventory-shaped rows for the given product ids, ordered by product id.</summary>
    private async Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsForAsync(HashSet<int> productIds)
    {
        if (productIds.Count == 0)
            return [];

        // Same inventory projection shape as ProductsDataService.GetInventoryRowsAsync; in-memory filter after SelectAllAsync.
        var rows = await products.SelectAllAsync(product => new ProductListItemViewModel(
            product.Id,
            product.Id,
            product.ModelNavigation.CategoryNavigation.Name,
            product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name,
            product.Barcode,
            product.ColorNavigation == null ? null : product.ColorNavigation.Name,
            product.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell),
            product.SecondHandProfile != null));

        return rows
            .Where(row => productIds.Contains(row.ProductId))
            .OrderBy(row => row.ProductId)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CreateCustomerAsync(CreateCustomerInputModel input)
    {
        var customer = new Customer
        {
            NationalId = input.NationalId.Trim(),
            PersonNavigation = new Person
            {
                FirstName = input.FirstName.Trim(),
                LastName = input.LastName.Trim(),
                PhoneNumber = input.PhoneNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
            },
        };

        var result = await customers.AddAsync(customer) > 0;
        if (!result)
        {
            Logger.LogWarning("Failed to add Customer");
            return new ServiceResult(false, "The customer could not be created.", null, null);
        }

        Logger.LogInformation("Added Customer Id={Id}", customer.Id);
        return new ServiceResult(true, null, null, customer.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CreateSellerAsync(CreateSellerInputModel input)
    {
        var seller = new Seller
        {
            EntityType = input.EntityType,
            PersonNavigation = new Person
            {
                FirstName = input.FirstName.Trim(),
                LastName = input.LastName.Trim(),
                PhoneNumber = input.PhoneNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
            },
        };

        var result = await sellers.AddAsync(seller) > 0;
        if (!result)
        {
            Logger.LogWarning("Failed to add Seller");
            return new ServiceResult(false, "The seller could not be created.", null, null);
        }

        Logger.LogInformation("Added Seller Id={Id}", seller.Id);
        return new ServiceResult(true, null, null, seller.Id);
    }
}
