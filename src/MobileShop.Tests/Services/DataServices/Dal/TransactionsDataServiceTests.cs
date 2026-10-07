
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

    private Transaction AddTransaction(
        Product product,
        int sellerId,
        int customerId,
        TransactionDirection direction,
        DateTime date,
        long price = 100)
    {
        var transaction = new Transaction
        {
            ProductId = product.Id,
            SellerId = sellerId,
            CustomerId = customerId,
            FinishedPrice = price,
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
    public async Task GetListAsync_sorts_by_price_in_both_directions()
    {
        var product1 = TestDataHelpers.CreateProduct(Context);
        var product2 = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");

        var low = AddTransaction(product1, seller.Id, customer.Id, TransactionDirection.Buy, new DateTime(2024, 1, 1), 100);
        var high = AddTransaction(product2, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 1, 2), 900);

        var ascending = await _service.GetListAsync("all", 50, true, "price");
        var descending = await _service.GetListAsync("all", 50, false, "price");

        Assert.Equal([low.Id, high.Id], ascending.Select(row => row.Id));
        Assert.Equal([high.Id, low.Id], descending.Select(row => row.Id));
    }

    [Fact]
    public async Task GetListAsync_sorts_by_product_in_both_directions()
    {
        var product1 = TestDataHelpers.CreateProduct(Context);
        var product2 = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        AddTransaction(product1, seller.Id, customer.Id, TransactionDirection.Buy, new DateTime(2024, 1, 1));
        AddTransaction(product2, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 1, 2));

        var rows = await _service.GetListAsync("all", 50, true, "product");
        var expected = rows.OrderBy(row => row.ProductLabel, StringComparer.OrdinalIgnoreCase).Select(row => row.Id).ToList();

        Assert.Equal(expected, rows.Select(row => row.Id));
        var descending = await _service.GetListAsync("all", 50, false, "product");
        Assert.Equal(expected.AsEnumerable().Reverse(), descending.Select(row => row.Id));
    }

    [Fact]
    public async Task GetListAsync_sorts_by_seller_in_both_directions()
    {
        var product1 = TestDataHelpers.CreateProduct(Context);
        var product2 = TestDataHelpers.CreateProduct(Context);
        var seller1 = AddSeller("Ali", "Zed");
        var seller2 = AddSeller("Behnam", "Ahmadi");
        var customer = AddCustomer("Sara", "Ahmadi");
        AddTransaction(product1, seller1.Id, customer.Id, TransactionDirection.Buy, new DateTime(2024, 1, 1));
        AddTransaction(product2, seller2.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 1, 2));

        var ascending = await _service.GetListAsync("all", 50, true, "seller");
        var descending = await _service.GetListAsync("all", 50, false, "seller");

        Assert.Equal(
            ascending.OrderBy(row => row.SellerLabel, StringComparer.OrdinalIgnoreCase).Select(row => row.Id),
            ascending.Select(row => row.Id));
        Assert.Equal(
            ascending.Select(row => row.Id).Reverse(),
            descending.Select(row => row.Id));
    }

    [Fact]
    public async Task GetListAsync_sorts_by_customer_in_both_directions()
    {
        var product1 = TestDataHelpers.CreateProduct(Context);
        var product2 = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer1 = AddCustomer("Sara", "Ahmadi");
        var customer2 = AddCustomer("Zara", "Ahmadi");
        AddTransaction(product1, seller.Id, customer1.Id, TransactionDirection.Buy, new DateTime(2024, 1, 1));
        AddTransaction(product2, seller.Id, customer2.Id, TransactionDirection.Sell, new DateTime(2024, 1, 2));

        var ascending = await _service.GetListAsync("all", 50, true, "customer");
        var descending = await _service.GetListAsync("all", 50, false, "customer");

        Assert.Equal(
            ascending.OrderBy(row => row.CustomerLabel, StringComparer.OrdinalIgnoreCase).Select(row => row.Id),
            ascending.Select(row => row.Id));
        Assert.Equal(
            ascending.Select(row => row.Id).Reverse(),
            descending.Select(row => row.Id));
    }

    [Fact]
    public async Task GetListAsync_unknown_sort_falls_back_to_date()
    {
        var product1 = TestDataHelpers.CreateProduct(Context);
        var product2 = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        var older = AddTransaction(product1, seller.Id, customer.Id, TransactionDirection.Buy, new DateTime(2024, 1, 1));
        var newer = AddTransaction(product2, seller.Id, customer.Id, TransactionDirection.Sell, new DateTime(2024, 6, 1));

        var rows = await _service.GetListAsync("all", 50, false, "bogus");

        Assert.Equal([newer.Id, older.Id], rows.Select(row => row.Id));
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
            Price = 250,
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.EntityId);
        var transaction = Context.Transactions.Single(t => t.Id == result.EntityId);
        Assert.Equal(TransactionDirection.Buy, transaction.Direction);
        Assert.Equal(seller.Id, transaction.SellerId);
        Assert.Equal(1, transaction.CustomerId);
        Assert.Equal(250, transaction.FinishedPrice);

        var duplicate = await _service.RecordBuyAsync(new BuyInputModel
        {
            ProductId = product.Id,
            SellerId = seller.Id,
            Price = 300,
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
            Price = 400,
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
            Price = 500,
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
            Price = -1,
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
            Price = -1,
        });

        Assert.False(result.Succeeded);
        Assert.Equal("The buy could not be recorded. Check the product and price.", result.Message);
        Assert.Empty(Context.Transactions);
    }

    [Fact]
    public async Task SearchSelectableProductsAsync_filters_by_direction_and_query()
    {
        var phoneProduct = TestDataHelpers.CreateProduct(Context);
        var appleIdProduct = TestDataHelpers.CreateProduct(Context);
        var seller = AddSeller("Ali", "Zed");
        var customer = AddCustomer("Sara", "Ahmadi");
        var phoneImei = TestDataHelpers.GenerateImei();
        var appleEmail = "apple@example.com";

        Context.Phones.Add(new Phone { ProductId = phoneProduct.Id, IMEI1 = phoneImei });
        Context.AppleIds.Add(new AppleId { ProductId = appleIdProduct.Id, Email = appleEmail });
        Context.SaveChanges();

        AddTransaction(phoneProduct, seller.Id, customer.Id, TransactionDirection.Buy, DateTime.UtcNow);
        AddTransaction(appleIdProduct, seller.Id, customer.Id, TransactionDirection.Sell, DateTime.UtcNow);

        var buyResults = await _service.SearchSelectableProductsAsync(TransactionDirection.Buy, appleEmail);
        var sellResults = await _service.SearchSelectableProductsAsync(TransactionDirection.Sell, phoneImei[6..]);
        var typeResults = await _service.SearchSelectableProductsAsync(TransactionDirection.Buy, "apple id");

        Assert.Single(buyResults);
        Assert.Equal(appleIdProduct.Id, buyResults[0].ProductId);
        Assert.Single(sellResults);
        Assert.Equal(phoneProduct.Id, sellResults[0].ProductId);
        Assert.Single(typeResults);
        Assert.Equal("Apple ID", typeResults[0].Type);
    }

    [Fact]
    public async Task SearchSelectableProductsAsync_matches_name_identifier_color_and_part_number_case_insensitively()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.SaveChanges();

        var color = new Color { Name = "Midnight Blue" };
        Context.Colors.Add(color);
        Context.SaveChanges();
        var partNumber = new PartNumber { ModelId = product.ModelId, Code = "PART-ABC" };
        Context.PartNumbers.Add(partNumber);
        Context.SaveChanges();

        var phone = Context.Phones.Single();
        phone.ProductNavigation.ColorId = color.Id;
        phone.PartNumberId = partNumber.Id;
        Context.SaveChanges();

        var name = await _service.SearchSelectableProductsAsync(TransactionDirection.Buy, "model-");
        var identifier = await _service.SearchSelectableProductsAsync(TransactionDirection.Buy, phone.IMEI1[..6]);
        var colorResult = await _service.SearchSelectableProductsAsync(TransactionDirection.Buy, "midnight blue");
        var partResult = await _service.SearchSelectableProductsAsync(TransactionDirection.Buy, "part-abc");

        Assert.Single(name);
        Assert.Single(identifier);
        Assert.Single(colorResult);
        Assert.Single(partResult);
    }

    [Fact]
    public async Task SearchSelectableProductsAsync_returns_first_results_for_empty_query_and_honors_take()
    {
        var product1 = TestDataHelpers.CreateProduct(Context);
        var product2 = TestDataHelpers.CreateProduct(Context);
        Context.Phones.Add(new Phone { ProductId = product1.Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.Phones.Add(new Phone { ProductId = product2.Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.SaveChanges();

        var all = await _service.SearchSelectableProductsAsync(TransactionDirection.Buy, null);
        var limited = await _service.SearchSelectableProductsAsync(TransactionDirection.Buy, " ", 1);

        Assert.Equal(2, all.Count);
        Assert.Single(limited);
        Assert.Equal(all[0].EntityId, limited[0].EntityId);
    }

    [Fact]
    public async Task SearchSelectableProductsAsync_filters_by_type_manufacturer_and_model()
    {
        var manufacturerA = new Manufacturer { Name = "Acme" };
        var manufacturerB = new Manufacturer { Name = "Other" };
        var phoneCategory = new Category { Name = "Phone" };
        Context.Manufacturers.AddRange(manufacturerA, manufacturerB);
        Context.Categories.Add(phoneCategory);
        Context.SaveChanges();

        var modelA1 = new Model { ManufacturerId = manufacturerA.Id, CategoryId = phoneCategory.Id, Name = "Model A1" };
        var modelA2 = new Model { ManufacturerId = manufacturerA.Id, CategoryId = phoneCategory.Id, Name = "Model A2" };
        var modelB = new Model { ManufacturerId = manufacturerB.Id, CategoryId = phoneCategory.Id, Name = "Model B" };
        Context.Models.AddRange(modelA1, modelA2, modelB);
        Context.SaveChanges();

        var products = new[]
        {
            new Product { ModelId = modelA1.Id, Barcode = "PHONE-A1", Price = 100 },
            new Product { ModelId = modelA2.Id, Barcode = "PHONE-A2", Price = 200 },
            new Product { ModelId = modelB.Id, Barcode = "PHONE-B", Price = 300 },
        };
        Context.Products.AddRange(products);
        Context.SaveChanges();
        Context.Phones.AddRange(
            new Phone { ProductId = products[0].Id, IMEI1 = TestDataHelpers.GenerateImei() },
            new Phone { ProductId = products[1].Id, IMEI1 = TestDataHelpers.GenerateImei() },
            new Phone { ProductId = products[2].Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.SaveChanges();

        var manufacturerRows = await _service.SearchSelectableProductsAsync(
            TransactionDirection.Buy, null, type: "Phone", manufacturerId: manufacturerA.Id);
        var modelRows = await _service.SearchSelectableProductsAsync(
            TransactionDirection.Buy, null, type: "Phone", manufacturerId: manufacturerA.Id, modelId: modelA2.Id);

        Assert.Equal(2, manufacturerRows.Count);
        var selected = Assert.Single(modelRows);
        Assert.Equal(products[1].Id, selected.ProductId);
        Assert.Equal(manufacturerA.Id, selected.ManufacturerId);
        Assert.Equal("Acme", selected.ManufacturerName);
        Assert.Equal(modelA2.Id, selected.ModelId);
        Assert.Equal("Model A2", selected.ModelName);
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
        Assert.Contains(selectable, row => row.ProductId == product.Id);
        Assert.Contains(selectable, row => row.ProductId == appleIdProduct.Id);

        AddTransaction(product, seller.Id, customer.Id, TransactionDirection.Sell, DateTime.UtcNow);
        var afterSell = await _service.GetSelectableProductsAsync(TransactionDirection.Sell);
        Assert.DoesNotContain(afterSell, row => row.ProductId == product.Id);
    }
    [Fact]
    public async Task GetSelectableProductsAsync_returns_unsold_new_devices()
    {
        var manufacturer = new Manufacturer { Name = "Test" }; Context.Manufacturers.Add(manufacturer); var categories = new[] { new Category { Name = "Tablet" }, new Category { Name = "SmartWatch" }, new Category { Name = "Laptop" } }; Context.Categories.AddRange(categories); Context.SaveChanges(); var models = categories.Select((c, i) => new Model { ManufacturerId = manufacturer.Id, CategoryId = c.Id, Name = $"Device {i}" }).ToArray(); Context.Models.AddRange(models); Context.SaveChanges();
        Context.Tablets.Add(new Tablet { ProductNavigation = new Product { ModelId = models[0].Id, Barcode = "T1", Price = 100 } }); Context.SmartWatches.Add(new SmartWatch { ProductNavigation = new Product { ModelId = models[1].Id, Barcode = "W1", Price = 200 } }); Context.Laptops.Add(new Laptop { ProductNavigation = new Product { ModelId = models[2].Id, Barcode = "L1", Price = 300 }, Cpu = "CPU", Gpu = "GPU", DisplaySize = 15.6m }); Context.SaveChanges();
        var rows = await _service.GetSelectableProductsAsync(TransactionDirection.Buy); Assert.Contains(rows, r => r.Type == "Tablet"); Assert.Contains(rows, r => r.Type == "Smart Watch"); Assert.Contains(rows, r => r.Type == "Laptop");
    }

    [Fact]
    public async Task GetSelectableProductsAsync_returns_accessory_profiles()
    {
        var manufacturer = new Manufacturer { Name = "Accessory Co" };
        var category = new Category { Name = "Cable" };
        Context.Manufacturers.Add(manufacturer);
        Context.Categories.Add(category);
        Context.SaveChanges();
        var model = new Model { ManufacturerId = manufacturer.Id, CategoryId = category.Id, Name = "USB-C Cable" };
        Context.Models.Add(model);
        Context.SaveChanges();
        var product = new Product { ModelId = model.Id, Barcode = "CABLE01", Price = 1000, CableProfile = new Cable { Connector1 = CableConnector.UsbC, Connector2 = CableConnector.UsbC, Length = 1 } };
        Context.Products.Add(product);
        Context.SaveChanges();

        var rows = await _service.GetSelectableProductsAsync(TransactionDirection.Buy);

        var row = Assert.Single(rows, r => r.ProductId == product.Id);
        Assert.Equal("Cable", row.Type);
        Assert.Equal(1000, row.SuggestedPrice);
    }

}
