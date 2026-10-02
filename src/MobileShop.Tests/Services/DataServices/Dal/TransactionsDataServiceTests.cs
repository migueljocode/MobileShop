using MobileShop.Models.ViewModels;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Services.PDF;

namespace MobileShop.Tests.Services.DataServices.Dal;

public class TransactionsDataServiceTests : RepoTestBase
{
    private readonly TransactionsDataService _service;
    private readonly FakePdfGenerator _pdfGenerator = new();

    private sealed class FakePdfGenerator : IPdfGenerator
    {
        public byte[] Generate(InvoiceViewModel model) => [1, 2, 3];

        public byte[] GenerateTransactionFactor(TransactionFactorViewModel model) => [4, 5, 6];
    }

    public TransactionsDataServiceTests()
    {
        _service = new TransactionsDataService(
            new BaseRepo<Transaction>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Customer>(Context),
            new BaseRepo<Phone>(Context),
            new BaseRepo<AppleId>(Context),
            new BaseRepo<Product>(Context),
            Context,
            _pdfGenerator,
            NullLogger<TransactionsDataService>.Instance);
    }

    private Seller AddSeller(string firstName, string lastName)
    {
        var person = new Person { FirstName = firstName, LastName = lastName, PhoneNumber = "09120000001" };
        Context.People.Add(person);
        Context.SaveChanges();

        var seller = new Seller { PersonId = person.Id, EntityType = SellerEntityType.Real };
        Context.Sellers.Add(seller);
        Context.SaveChanges();
        return seller;
    }

    private Customer AddCustomer(string firstName, string lastName)
    {
        var person = new Person { FirstName = firstName, LastName = lastName, PhoneNumber = "09120000002" };
        Context.People.Add(person);
        Context.SaveChanges();

        var customer = new Customer { PersonId = person.Id, NationalId = Guid.NewGuid().ToString("N")[..10] };
        Context.Customers.Add(customer);
        Context.SaveChanges();
        return customer;
    }

    private Transaction AddTransaction(Product product, int sellerId, int customerId, TransactionDirection direction, DateTime date)
    {
        var transaction = new Transaction
        {
            ProductId = product.Id,
            SellerId = sellerId,
            CustomerId = customerId,
            FinishedPrice = 100m,
            Date = date,
            Direction = direction,
        };
        Context.Transactions.Add(transaction);
        Context.SaveChanges();
        return transaction;
    }

    [Fact]
    public async Task GetListAsync_filters_by_direction_and_orders_by_date()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var product2 = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");

        var older = AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Buy, new DateTime(2024, 1, 1));
        var newer = AddTransaction(product2, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 6, 1));

        var allDesc = await _service.GetListAsync("all", 50, false);
        Assert.Equal(2, allDesc.Count);
        Assert.Equal(newer.Id, allDesc[0].Id);
        Assert.Equal(older.Id, allDesc[1].Id);

        var allAsc = await _service.GetListAsync("all", 50, true);
        Assert.Equal(older.Id, allAsc[0].Id);

        var buyOnly = await _service.GetListAsync("buy", 50, false);
        Assert.Single(buyOnly);
        Assert.Equal(older.Id, buyOnly[0].Id);

        var sellOnly = await _service.GetListAsync("sell", 50, false);
        Assert.Single(sellOnly);
        Assert.Equal(newer.Id, sellOnly[0].Id);
    }

    [Fact]
    public async Task GetListAsync_clamps_take_to_at_least_one()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Buy, DateTime.UtcNow);

        var rows = await _service.GetListAsync("all", 0, false);

        Assert.Single(rows);
    }

    [Fact]
    public async Task GetDetailsAsync_projects_transaction_and_returns_null_for_unknown_id()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        var transaction = AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 3, 2));

        var details = await _service.GetDetailsAsync(transaction.Id);

        Assert.NotNull(details);
        Assert.Equal(TransactionDirection.Sell, details!.Direction);
        Assert.Equal("Ali Zed", details.SellerLabel);
        Assert.Equal("Sara Ahmadi", details.CustomerLabel);

        Assert.Null(await _service.GetDetailsAsync(999));
    }

    [Fact]
    public async Task GetSellersAsync_and_GetCustomersAsync_return_party_options()
    {
        AddSeller("Ali", "Zed");
        AddCustomer("Sara", "Ahmadi");

        var sellers = await _service.GetSellersAsync();
        var customers = await _service.GetCustomersAsync();

        Assert.Single(sellers);
        Assert.Equal("Ali Zed", sellers[0].Label);
        Assert.Single(customers);
        Assert.Equal("Sara Ahmadi", customers[0].Label);
    }

    [Fact]
    public async Task GetTransactionFactorPdfAsync_returns_null_for_unknown_id()
    {
        Assert.Null(await _service.GetTransactionFactorPdfAsync(999));
    }

    [Fact]
    public async Task GetTransactionFactorPdfAsync_returns_bytes_for_existing_transaction()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        var transaction = AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, DateTime.UtcNow);

        var bytes = await _service.GetTransactionFactorPdfAsync(transaction.Id);

        Assert.NotNull(bytes);
        Assert.Equal(new byte[] { 4, 5, 6 }, bytes);
    }

    [Fact]
    public async Task RecordBuyAsync_persists_with_shop_customer_and_rejects_duplicate_buy()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var product = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");

        var result = await _service.RecordBuyAsync(new BuyInputModel
        {
            ProductId = product.Id,
            SellerId = seller.Id,
            Price = 250m,
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.EntityId);
        var transaction = Context.Transactions.Single(t => t.Id == result.EntityId);
        Assert.Equal(TransactionDirection.Buy, transaction.Direction);
        Assert.Equal(seller.Id, transaction.SellerId);
        Assert.Equal(1, transaction.CustomerId);
        Assert.Equal(250m, transaction.FinishedPrice);

        var duplicate = await _service.RecordBuyAsync(new BuyInputModel
        {
            ProductId = product.Id,
            SellerId = seller.Id,
            Price = 300m,
        });

        Assert.False(duplicate.Succeeded);
        Assert.Equal("The buy could not be recorded. Check the product and price.", duplicate.Message);
    }

    [Fact]
    public async Task RecordSellAsync_persists_with_shop_seller_and_rejects_duplicate_sell()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var product = TestDataHelpers.CreateProduct(Context);
        var customer = AddCustomer("Sara", "Ahmadi");

        var result = await _service.RecordSellAsync(new SellInputModel
        {
            ProductId = product.Id,
            CustomerId = customer.Id,
            Price = 400m,
        });

        Assert.True(result.Succeeded);
        var transaction = Context.Transactions.Single(t => t.Id == result.EntityId);
        Assert.Equal(TransactionDirection.Sell, transaction.Direction);
        Assert.Equal(1, transaction.SellerId);
        Assert.Equal(customer.Id, transaction.CustomerId);

        var duplicate = await _service.RecordSellAsync(new SellInputModel
        {
            ProductId = product.Id,
            CustomerId = customer.Id,
            Price = 500m,
        });

        Assert.False(duplicate.Succeeded);
        Assert.Equal("The sale could not be recorded. Check the product and price.", duplicate.Message);
    }

    [Fact]
    public async Task RecordSellAsync_rejects_negative_price()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var product = TestDataHelpers.CreateProduct(Context);
        var customer = AddCustomer("Sara", "Ahmadi");

        var result = await _service.RecordSellAsync(new SellInputModel
        {
            ProductId = product.Id,
            CustomerId = customer.Id,
            Price = -1m,
        });

        Assert.False(result.Succeeded);
        Assert.Empty(Context.Transactions);
    }

    [Fact]
    public async Task RecordBuyAsync_rejects_negative_price()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        var product = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");

        var result = await _service.RecordBuyAsync(new BuyInputModel
        {
            ProductId = product.Id,
            SellerId = seller.Id,
            Price = -1m,
        });

        Assert.False(result.Succeeded);
        Assert.Equal("The buy could not be recorded. Check the product and price.", result.Message);
        Assert.Empty(Context.Transactions);
    }

    [Fact]
    public async Task GetSelectableProductsAsync_returns_unsold_phones_and_apple_ids_ordered_by_name()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var appleIdProduct = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");

        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.AppleIds.Add(new AppleId { ProductId = appleIdProduct.Id, Email = "a@b.c" });
        Context.SaveChanges();

        var selectable = await _service.GetSelectableProductsAsync(TransactionDirection.Buy);

        Assert.Equal(2, selectable.Count);
        Assert.Contains(selectable, row => row.Type == "Phone");
        Assert.Contains(selectable, row => row.Type == "Apple ID");
        Assert.True(selectable.SequenceEqual(selectable.OrderBy(row => row.Name)));

        // The phone already has a Buy transaction, so it is no longer selectable for Buy.
        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Buy, DateTime.UtcNow);

        var afterBuy = await _service.GetSelectableProductsAsync(TransactionDirection.Buy);

        Assert.Single(afterBuy);
        Assert.Equal("Apple ID", afterBuy[0].Type);
    }

    [Fact]
    public async Task GetSelectableProductsAsync_includes_glass_products_and_excludes_glass_after_buy()
    {
        var glassCategory = new Category { Name = "Glass" };
        var manufacturer = new Manufacturer { Name = "Apple" };
        Context.Categories.Add(glassCategory);
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();
        var model = new Model { ManufacturerId = manufacturer.Id, CategoryId = glassCategory.Id, Name = "iPhone 16 Glass" };
        Context.Models.Add(model);
        Context.SaveChanges();
        var product = new Product { ModelId = model.Id, Barcode = "GLASS123456", Price = 123m, GlassProfile = new Glass() };
        Context.Products.Add(product);
        Context.SaveChanges();

        var selectable = await _service.GetSelectableProductsAsync(TransactionDirection.Buy);
        var glass = Assert.Single(selectable);
        Assert.Equal("Glass", glass.Type);
        Assert.Equal("Barcode: GLASS123456", glass.Identifier);
        Assert.Equal(123m, glass.SuggestedPrice);

        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Buy, DateTime.UtcNow);

        Assert.Empty(await _service.GetSelectableProductsAsync(TransactionDirection.Buy));
    }

    [Fact]
    public async Task GetSelectableProductsAsync_carries_catalog_price_as_suggested_price()
    {
        var product = TestDataHelpers.CreateProduct(Context, price: 1234.50m);
        var appleIdProduct = TestDataHelpers.CreateProduct(Context, price: 99.99m);

        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.AppleIds.Add(new AppleId { ProductId = appleIdProduct.Id, Email = "a@b.c" });
        Context.SaveChanges();

        var selectable = await _service.GetSelectableProductsAsync(TransactionDirection.Buy);

        Assert.Equal(2, selectable.Count);
        var phoneRow = Assert.Single(selectable, row => row.Type == "Phone");
        Assert.Equal(1234.50m, phoneRow.SuggestedPrice);
        var appleIdRow = Assert.Single(selectable, row => row.Type == "Apple ID");
        Assert.Equal(99.99m, appleIdRow.SuggestedPrice);
    }

    [Fact]
    public async Task GenerateListFactorPdfAsync_fails_with_filter_message_when_snapshot_is_empty()
    {
        var result = await _service.GenerateListFactorPdfAsync("all", 50, false, []);

        Assert.False(result.Succeeded);
        Assert.Null(result.Bytes);
        Assert.Equal("No transactions match the current filters.", result.Error);
    }

    [Fact]
    public async Task GenerateListFactorPdfAsync_fails_for_non_positive_selected_id()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Buy, DateTime.UtcNow);

        var result = await _service.GenerateListFactorPdfAsync("all", 50, false, [0]);

        Assert.False(result.Succeeded);
        Assert.Null(result.Bytes);
        Assert.Equal("Selected transaction identifiers must be positive numbers.", result.Error);
    }

    [Fact]
    public async Task GenerateListFactorPdfAsync_fails_for_missing_selected_id_without_partial_bytes()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        var transaction = AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Buy, DateTime.UtcNow);

        var result = await _service.GenerateListFactorPdfAsync("all", 50, false, [transaction.Id, 12345]);

        Assert.False(result.Succeeded);
        Assert.Null(result.Bytes);
        Assert.Equal("These selected transactions no longer exist: 12345.", result.Error);
    }

    [Fact]
    public async Task GenerateListFactorPdfAsync_succeeds_with_selection_and_without_selection()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        var transaction = AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Buy, DateTime.UtcNow);

        var selected = await _service.GenerateListFactorPdfAsync("all", 50, false, [transaction.Id]);

        Assert.True(selected.Succeeded);
        Assert.Null(selected.Error);
        Assert.Equal(new byte[] { 4, 5, 6 }, selected.Bytes);

        var filtered = await _service.GenerateListFactorPdfAsync("buy", 50, false, []);

        Assert.True(filtered.Succeeded);
        Assert.NotNull(filtered.Bytes);
    }

    [Fact]
    public async Task GenerateInvoicePdfAsync_returns_bytes_and_throws_for_unknown_transaction()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        var transaction = AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, DateTime.UtcNow);

        var invoice = await _service.GetInvoiceAsync(transaction.Id);
        Assert.NotNull(invoice);
        Assert.Equal("Sara Ahmadi", invoice!.BuyerName);

        var bytes = await _service.GenerateInvoicePdfAsync(transaction.Id);
        Assert.Equal(new byte[] { 1, 2, 3 }, bytes);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GenerateInvoicePdfAsync(999));
        Assert.Null(await _service.GetInvoiceAsync(999));
    }
}
