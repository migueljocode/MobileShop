using Microsoft.Extensions.DependencyInjection;

namespace MobileShop.Tests.Dal.Initialization;

/// <summary>
/// Stage 4 seed-data coverage: proves the bundled <c>sample-data.json</c> can be loaded
/// repeatedly by the development initializer against a real SQLite database (the same
/// EnsureCreated + seed path <c>DatabaseInitializer.InitializeForDevelopment</c> runs at
/// startup), and that the seeded records satisfy every configured relationship, unique
/// index, required field, enum, and shop sentinel the services depend on.
/// </summary>
public class SampleDataSeedTests : IDisposable
{
    private readonly string _databaseFile =
        Path.Combine(Path.GetTempPath(), $"mobileshop-seed-{Guid.NewGuid():N}.db");

    private string ConnectionString => $"Data Source={_databaseFile};Pooling=False";

    public void Dispose()
    {
        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            var file = _databaseFile + suffix;
            if (!File.Exists(file))
            {
                continue;
            }

            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
                // Best-effort temp-file cleanup only.
            }
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Development_startup_recreates_and_seeds_database_with_expected_counts()
    {
        // Act: the literal development-startup entry point (drop + create + seed).
        using var provider = BuildProvider();
        DatabaseInitializer.InitializeForDevelopment(provider);

        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Assert: every collection the loader consumes lands with the expected record count.
        Assert.Equal(7, context.People.Count());
        Assert.Equal(3, context.Sellers.Count());
        Assert.Equal(4, context.Customers.Count());
        Assert.Equal(1, context.Users.Count());
        Assert.Equal(6, context.Manufacturers.Count());
        Assert.Equal(7, context.Categories.Count());
        Assert.Equal(6, context.Colors.Count());
        Assert.Equal(4, context.StorageCapacities.Count());
        Assert.Equal(16, context.Models.Count());
        Assert.Equal(17, context.Products.Count());
        Assert.Equal(26, context.Transactions.Count());
        Assert.Equal(3, context.AppleIds.Count());
        Assert.Equal(7, context.Phones.Count());
        Assert.Equal(2, context.SecondHands.Count());
        Assert.Equal(4, context.Guarantees.Count());
        Assert.Equal(7, context.DeviceSpecs.Count());
        Assert.Equal(2, context.Cables.Count());
        Assert.Equal(2, context.Chargers.Count());
        Assert.Equal(1, context.PowerBanks.Count());
        Assert.Equal(1, context.Cases.Count());
        Assert.Equal(1, context.CaseModelFits.Count());
        Assert.Equal(2, context.Glasses.Count());
        Assert.Equal(2, context.GlassModelFits.Count());
        Assert.Equal(4, context.Employees.Count());

        // Collections intentionally left empty for empty-state page coverage.
        Assert.Equal(0, context.Tablets.Count());
        Assert.Equal(0, context.SmartWatches.Count());
        Assert.Equal(0, context.Laptops.Count());
        Assert.Equal(0, context.PortableStorages.Count());
    }

    [Fact]
    public void Seeded_transactions_resolve_shop_sentinels_and_navigations()
    {
        using var context = SeedFreshDatabase();

        var transactions = context.Transactions
            .Include(transaction => transaction.SellerNavigation)
                .ThenInclude(seller => seller.PersonNavigation)
            .Include(transaction => transaction.CustomerNavigation)
                .ThenInclude(customer => customer.PersonNavigation)
            .Include(transaction => transaction.ProductNavigation)
                .ThenInclude(product => product.ModelNavigation)
                    .ThenInclude(model => model.ManufacturerNavigation)
            .ToList();

        Assert.Equal(26, transactions.Count);
        Assert.Equal(14, transactions.Count(transaction => transaction.Direction == TransactionDirection.Buy));
        Assert.Equal(12, transactions.Count(transaction => transaction.Direction == TransactionDirection.Sell));

        // Profit/loss and factor rows render seller/customer/product names, so every
        // navigation in the chain must resolve to a real record.
        Assert.All(transactions, transaction =>
        {
            Assert.NotNull(transaction.SellerNavigation.PersonNavigation);
            Assert.NotNull(transaction.CustomerNavigation.PersonNavigation);
            Assert.NotNull(transaction.ProductNavigation.ModelNavigation.ManufacturerNavigation);
            Assert.False(string.IsNullOrWhiteSpace(transaction.SellerNavigation.PersonNavigation.FirstName));
            Assert.False(string.IsNullOrWhiteSpace(transaction.CustomerNavigation.PersonNavigation.LastName));
            Assert.False(string.IsNullOrWhiteSpace(transaction.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name));
        });

        // Shop sentinel rule relied on by TransactionDataService (Buy => shop is customer,
        // Sell => shop is seller; both sentinel ids point at person 1).
        Assert.All(transactions.Where(transaction => transaction.Direction == TransactionDirection.Buy),
            transaction => Assert.Equal(1, transaction.CustomerId));
        Assert.All(transactions.Where(transaction => transaction.Direction == TransactionDirection.Sell),
            transaction => Assert.Equal(1, transaction.SellerId));
        Assert.Equal(1, context.Sellers.Single(seller => seller.Id == 1).PersonId);
        Assert.Equal(1, context.Customers.Single(customer => customer.Id == 1).PersonId);
    }

    [Fact]
    public void Seeded_catalog_relationships_resolve()
    {
        using var context = SeedFreshDatabase();

        // Unique indexes (barcode, username, manufacturer name, model+manufacturer, gb,
        // imei1, fit pairs) were enforced by SQLite while seeding - reaching this point
        // without a SqliteException proves no duplicate slipped in.
        var products = context.Products
            .Include(product => product.ModelNavigation)
                .ThenInclude(model => model.ManufacturerNavigation)
            .Include(product => product.ModelNavigation)
                .ThenInclude(model => model.CategoryNavigation)
            .Include(product => product.ColorNavigation)
            .ToList();

        Assert.Equal(17, products.Count);
        Assert.All(products, product =>
        {
            Assert.NotNull(product.ModelNavigation);
            Assert.NotNull(product.ModelNavigation.ManufacturerNavigation);
            Assert.NotNull(product.ModelNavigation.CategoryNavigation);
        });
        Assert.Equal(17, products.Select(product => product.Barcode).Distinct().Count());

        var deviceSpecs = context.DeviceSpecs
            .Include(spec => spec.StorageCapacityNavigation)
            .ToList();
        Assert.Equal(7, deviceSpecs.Count);
        Assert.All(deviceSpecs, spec =>
        {
            Assert.NotNull(spec.StorageCapacityNavigation);
            Assert.True(spec.StorageCapacityNavigation.Gb > 0);
        });

        var employees = context.Employees.Include(employee => employee.PersonNavigation).ToList();
        Assert.Equal(4, employees.Count);
        Assert.All(employees, employee => Assert.NotNull(employee.PersonNavigation));

        // Glass fits must reference a Phone/Tablet/SmartWatch model (service rule).
        var glassFits = context.GlassModelFits
            .Include(fit => fit.ModelNavigation)
                .ThenInclude(model => model.CategoryNavigation)
            .ToList();
        Assert.Equal(2, glassFits.Count);
        Assert.All(glassFits, fit =>
            Assert.Contains(fit.ModelNavigation.CategoryNavigation.Name,
                new[] { "Phone", "Tablet", "SmartWatch" }));

        var caseFits = context.CaseModelFits.Include(fit => fit.CaseNavigation).ToList();
        Assert.Single(caseFits);
        Assert.All(caseFits, fit => Assert.NotNull(fit.CaseNavigation));

        var phones = context.Phones.Include(phone => phone.ProductNavigation).ToList();
        Assert.Equal(7, phones.Count);
        Assert.All(phones, phone => Assert.NotNull(phone.ProductNavigation));
        Assert.Equal(7, phones.Select(phone => phone.IMEI1).Distinct().Count());
    }

    [Fact]
    public void Seeded_records_cover_empty_single_filtered_and_multi_cases()
    {
        using var context = SeedFreshDatabase();

        // Empty case: pages that list tablets / smart watches / laptops / portable storages.
        Assert.Equal(0, context.Tablets.Count());
        Assert.Equal(0, context.SmartWatches.Count());
        Assert.Equal(0, context.Laptops.Count());
        Assert.Equal(0, context.PortableStorages.Count());

        // Single-record case: power banks, cases and case fits.
        Assert.Equal(1, context.PowerBanks.Count());
        Assert.Equal(1, context.Cases.Count());
        Assert.Equal(1, context.CaseModelFits.Count());

        // Multi-record case: phones, products, transactions.
        Assert.True(context.Phones.Count() >= 2);
        Assert.True(context.Products.Count() >= 2);
        Assert.True(context.Transactions.Count() >= 2);

        // Direction filter: both Buy and Sell rows exist.
        Assert.Contains(context.Transactions, transaction => transaction.Direction == TransactionDirection.Buy);
        Assert.Contains(context.Transactions, transaction => transaction.Direction == TransactionDirection.Sell);

        // Selectable-for-buy products (never bought) and unsold products both exist,
        // so RecordBuy / RecordSell selections and IsSold inventory flags have both states.
        var boughtProductIds = context.Transactions
            .Where(transaction => transaction.Direction == TransactionDirection.Buy)
            .Select(transaction => transaction.ProductId)
            .ToHashSet();
        var soldProductIds = context.Transactions
            .Where(transaction => transaction.Direction == TransactionDirection.Sell)
            .Select(transaction => transaction.ProductId)
            .ToHashSet();
        var allProductIds = context.Products.Select(product => product.Id).ToHashSet();
        Assert.Subset(allProductIds, boughtProductIds);
        Assert.Subset(allProductIds, soldProductIds);
        Assert.True(allProductIds.Except(boughtProductIds).Any(), "expected at least one never-bought product");
        Assert.True(allProductIds.Except(soldProductIds).Any(), "expected at least one unsold product");

        // Guarantee date filter: one already-expired and one still-active record
        // (fixed cutoffs so the assertions stay stable over time).
        Assert.Contains(context.Guarantees, guarantee => guarantee.ExpirationDate < new DateTime(2026, 1, 1));
        Assert.Contains(context.Guarantees, guarantee => guarantee.ExpirationDate > new DateTime(2027, 1, 1));

        // Employee distribution: active and inactive rows both exist.
        Assert.Equal(3, context.Employees.Count(employee => employee.IsActive));
        Assert.Equal(1, context.Employees.Count(employee => !employee.IsActive));
        Assert.Equal(100, context.Employees
            .Where(employee => employee.IsActive)
            .Sum(employee => employee.SharePercent));

        // Second-hand rows: one already sold and one still available.
        var soldProductIdsForSecondHand = context.Transactions
            .Where(transaction => transaction.Direction == TransactionDirection.Sell)
            .Select(transaction => transaction.ProductId)
            .ToHashSet();
        var secondHands = context.SecondHands.ToList();
        Assert.Contains(secondHands, secondHand => soldProductIdsForSecondHand.Contains(secondHand.ProductId));
        Assert.Contains(secondHands, secondHand => !soldProductIdsForSecondHand.Contains(secondHand.ProductId));

        // Apple ID rows: one on a sold product and one still unsold.
        Assert.Contains(context.AppleIds, appleId =>
            soldProductIds.Contains(appleId.ProductId));
        Assert.Contains(context.AppleIds, appleId =>
            !soldProductIds.Contains(appleId.ProductId));
    }

    [Fact]
    public void Seeded_credentials_keep_required_formats()
    {
        using var context = SeedFreshDatabase();

        // The seeded dev admin must exist for /Account/Profile; its stored value must not
        // be the plaintext dev password (the placeholder is replaced by EnsureAdminUser
        // with a real hash at startup).
        var admin = context.Users.Single(user => user.Username == "admin");
        Assert.False(string.IsNullOrWhiteSpace(admin.PasswordHash));
        Assert.NotEqual("Admin@123", admin.PasswordHash);
        Assert.Equal(1, context.Users.Count());

        // Apple ID inventory passwords stay plaintext by design (external credentials
        // must stay retrievable) and keep a valid email attached.
        Assert.Equal(3, context.AppleIds.Count());
        Assert.All(context.AppleIds.ToList(), appleId =>
        {
            Assert.False(string.IsNullOrWhiteSpace(appleId.Password));
            Assert.Contains("@", appleId.Email);
        });
    }

    [Fact]
    public void Seed_data_can_be_reloaded_repeatedly_with_stable_ids()
    {
        using (var context = SeedFreshDatabase())
        {
            // Second SeedIfEmpty must be a no-op (no duplicated rows).
            var productsBefore = context.Products.Count();
            SampleDataInitializer.SeedIfEmpty(context);
            Assert.Equal(productsBefore, context.Products.Count());
            Assert.Equal(7, context.People.Count());
        }

        // The destructive dev reseed runs in its own scope with a fresh context (the same
        // way dev tooling invokes it); ClearData issues direct SQL deletes, so reseeding on
        // a context still tracking the first seed's entities would conflict by design.
        using (var freshContext = CreateContext())
        {
            SampleDataInitializer.ClearAndReseedDatabase(freshContext);
            Assert.Equal(7, freshContext.People.Count());
            Assert.Equal(17, freshContext.Products.Count());
            Assert.Equal(26, freshContext.Transactions.Count());
            Assert.Equal(4, freshContext.Employees.Count());
            Assert.Equal(1, freshContext.Users.Single(user => user.Username == "admin").Id);
            Assert.Equal(1, freshContext.Sellers.Single(seller => seller.Id == 1).PersonId);
            Assert.Equal(17, freshContext.Products.Select(product => product.Id).Distinct().Count());
        }
    }

    private ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(ConnectionString));
        return services.BuildServiceProvider();
    }

    private AppDbContext SeedFreshDatabase()
    {
        var context = CreateContext();

        // Mirrors DatabaseInitializer.InitializeForDevelopment: drop, create, seed.
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        SampleDataInitializer.SeedIfEmpty(context);
        return context;
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(ConnectionString)
            .Options;
        return new AppDbContext(options);
    }
}
