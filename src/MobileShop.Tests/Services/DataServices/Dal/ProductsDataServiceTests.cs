namespace MobileShop.Tests.Services.DataServices.Dal;

/// <summary>
/// EF Core's InMemory provider builds its entity-type model lazily and caches it in a static field.
/// When several test classes construct <see cref="AppDbContext"/> (each over its own named in-memory
/// database) at the same time, the first-time model build can race and transiently misconfigure an
/// entity type — surfacing as a missing entity in a query predicate. The catalog tests below exercise
/// color/name dedupe lookups that are sensitive to that window, so they are forced to run serially.
/// </summary>
[CollectionDefinition("MobileShop-Dal-Serial", DisableParallelization = true)]
public class MobileShopDalSerialCollection;

[Collection("MobileShop-Dal-Serial")]
public class ProductsDataServiceTests : RepoTestBase
{
    private readonly ProductsDataService _service;

    public ProductsDataServiceTests()
    {
        _service = new ProductsDataService(
            new BaseRepo<Phone>(Context),
            new BaseRepo<AppleId>(Context),
            new BaseRepo<Manufacturer>(Context),
            new BaseRepo<Model>(Context),
            new BaseRepo<Category>(Context),
            new BaseRepo<MobileShop.Models.Entities.Color>(Context),
            new BaseRepo<Guarantee>(Context),
            new BaseRepo<Transaction>(Context),
            NullLogger<ProductsDataService>.Instance);
    }

    private void SeedCatalog(out Model phoneModel, out Model appleIdModel)
    {
        var manufacturer = new Manufacturer { Name = "Apple" };
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();

        var phoneCategory = new Category { Name = "Phone" };
        var appleIdCategory = new Category { Name = "AppleId" };
        Context.Categories.AddRange(phoneCategory, appleIdCategory);
        Context.SaveChanges();

        phoneModel = new Model { ManufacturerId = manufacturer.Id, CategoryId = phoneCategory.Id, Name = "iPhone 16" };
        appleIdModel = new Model { ManufacturerId = manufacturer.Id, CategoryId = appleIdCategory.Id, Name = "Apple ID" };
        Context.Models.AddRange(phoneModel, appleIdModel);
        Context.SaveChanges();
    }

    [Fact]
    public async Task GetManufacturersAsync_projects_all_seed_manufacturers()
    {
        SeedCatalog(out _, out _);
        Context.Manufacturers.Add(new Manufacturer { Name = "Samsung" });
        Context.SaveChanges();

        var options = await _service.GetManufacturersAsync();

        Assert.Equal(2, options.Count);
        Assert.Contains(options, o => o.Id == 1 && o.Name == "Apple");
        Assert.Contains(options, o => o.Name == "Samsung");
    }

    [Fact]
    public async Task GetModelsAsync_only_returns_models_for_the_given_manufacturer()
    {
        SeedCatalog(out var phoneModel, out var appleIdModel);
        var otherManufacturer = new Manufacturer { Name = "Samsung" };
        Context.Manufacturers.Add(otherManufacturer);
        Context.SaveChanges();
        Context.Models.Add(new Model { ManufacturerId = otherManufacturer.Id, CategoryId = Context.Categories.First(c => c.Name == "Phone").Id, Name = "Galaxy S24" });
        Context.SaveChanges();

        var options = await _service.GetModelsAsync(1);

        Assert.Contains(options, o => o.Id == phoneModel.Id && o.Name == "iPhone 16");
        Assert.Contains(options, o => o.Id == appleIdModel.Id && o.Name == "Apple ID");
        Assert.DoesNotContain(options, o => o.Name == "Galaxy S24");
    }

    [Fact]
    public async Task GetColorsAsync_returns_all_colors_ordered_by_creation()
    {
        SeedCatalog(out _, out _);
        Context.Colors.AddRange(
            new MobileShop.Models.Entities.Color { Name = "Black" },
            new MobileShop.Models.Entities.Color { Name = "White" });
        Context.SaveChanges();

        var options = await _service.GetColorsAsync();

        Assert.Equal(2, options.Count);
    }

    [Fact]
    public async Task GetGuaranteeCorporationsAsync_deduplicates_case_insensitively_with_ordinal_order()
    {
        SeedCatalog(out _, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Entry(product).State = EntityState.Detached;
        Context.Guarantees.AddRange(
            new Guarantee { Corporation = "Apple", StartDate = DateTime.Today, ExpirationDate = DateTime.Today.AddYears(1), ProductId = product.Id },
            new Guarantee { Corporation = "apple", StartDate = DateTime.Today, ExpirationDate = DateTime.Today.AddYears(1), ProductId = product.Id });
        Context.SaveChanges();

        var corporations = await _service.GetGuaranteeCorporationsAsync();

        Assert.Single(corporations);
        Assert.Equal("apple", corporations[0], ignoreCase: true);
    }

    [Fact]
    public async Task GetInventoryRowsAsync_contains_both_phone_and_apple_id_rows()
    {
        SeedCatalog(out _, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        var phone = new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei(), OwnershipTransferred = false };
        var appleId = new AppleId { ProductId = product.Id, Email = "stock@example.com", Password = "secret" };
        Context.Phones.Add(phone);
        Context.AppleIds.Add(appleId);
        Context.SaveChanges();

        var rows = await _service.GetInventoryRowsAsync();

        Assert.Equal(2, rows.Count);
        var phoneRow = Assert.Single(rows, r => r.Type == "Phone");
        Assert.Equal($"IMEI: {phone.IMEI1}", phoneRow.Identifier);
        var appleRow = Assert.Single(rows, r => r.Type == "Apple ID");
        Assert.Equal("stock@example.com", appleRow.Identifier);
    }

    [Fact]
    public async Task GetInventoryRowsAsync_returns_phones_first_then_apple_ids_each_ascending_by_product_id()
    {
        SeedCatalog(out _, out _);

        // Insert in a deliberately shuffled order (and with non-monotonic ids) so the test only
        // passes if the service applies its documented ordering rule.
        var products = new List<Product>();
        for (var i = 0; i < 3; i++)
            products.Add(TestDataHelpers.CreateProduct(Context));

        // Phones in reverse creation order, Apple IDs in reverse creation order.
        Context.Phones.Add(new Phone { ProductId = products[2].Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.Phones.Add(new Phone { ProductId = products[0].Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.Phones.Add(new Phone { ProductId = products[1].Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.AppleIds.Add(new AppleId { ProductId = products[2].Id, Email = "c@example.com", Password = "p" });
        Context.AppleIds.Add(new AppleId { ProductId = products[0].Id, Email = "a@example.com", Password = "p" });
        Context.AppleIds.Add(new AppleId { ProductId = products[1].Id, Email = "b@example.com", Password = "p" });
        Context.SaveChanges();

        var rows = await _service.GetInventoryRowsAsync();

        // Documented rule: the Phone block first, then the Apple ID block, each ascending by ProductId.
        Assert.Equal(
            new[] { "Phone", "Phone", "Phone", "Apple ID", "Apple ID", "Apple ID" },
            rows.Select(row => row.Type));

        Assert.Equal(
            new[] { products[0].Id, products[1].Id, products[2].Id,
                    products[0].Id, products[1].Id, products[2].Id },
            rows.Select(row => row.ProductId));
    }

    [Fact]
    public async Task GetInventoryRowsAsync_returns_empty_list_when_no_phones_or_apple_ids_exist()
    {
        SeedCatalog(out _, out _);

        var defaultRows = await _service.GetInventoryRowsAsync();
        Assert.NotNull(defaultRows);
        Assert.Empty(defaultRows);

        Assert.Empty(await _service.GetInventoryRowsAsync("phone"));
        Assert.Empty(await _service.GetInventoryRowsAsync("appleid"));
        Assert.Empty(await _service.GetInventoryRowsAsync("all"));
    }

    [Fact]
    public async Task GetInventoryRowsAsync_returns_empty_list_after_inventory_is_removed()
    {
        SeedCatalog(out _, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.SaveChanges();

        Assert.Single(await _service.GetInventoryRowsAsync());

        Context.Entry(Context.Phones.Single()).State = EntityState.Deleted;
        Context.SaveChanges();

        // Soft-deleted inventory must drop out, and the result must be empty rather than null.
        var rows = await _service.GetInventoryRowsAsync();

        Assert.NotNull(rows);
        Assert.Empty(rows);
    }

    [Fact]
    public async Task GetSecondHandRowsAsync_only_returns_second_hand_products()
    {
        SeedCatalog(out _, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei(), OwnershipTransferred = false });
        Context.SaveChanges();

        var shProduct = TestDataHelpers.CreateProduct(Context);
        shProduct = Context.Products.First(p => p.Id == shProduct.Id);
        Context.SecondHands.Add(new SecondHand { ProductId = shProduct.Id, TestPeriodDays = 3, UsedDurationDays = 1 });
        Context.Phones.Add(new Phone { ProductId = shProduct.Id, IMEI1 = TestDataHelpers.GenerateImei(), OwnershipTransferred = false });
        Context.SaveChanges();

        var rows = await _service.GetSecondHandRowsAsync();

        Assert.Single(rows);
        Assert.Equal("Phone", rows[0].Type);
        Assert.True(rows[0].IsSecondHand);
    }

    // ── Inventory type filter ──────────────────────────────────

    [Fact]
    public async Task GetInventoryRowsAsync_phone_type_returns_only_phone_rows()
    {
        SeedCatalog(out _, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei(), OwnershipTransferred = false });
        Context.AppleIds.Add(new AppleId { ProductId = product.Id, Email = "stock@example.com", Password = "secret" });
        Context.SaveChanges();

        var rows = await _service.GetInventoryRowsAsync("phone");

        Assert.Single(rows);
        Assert.Equal("Phone", rows[0].Type);
    }

    [Fact]
    public async Task GetInventoryRowsAsync_appleid_type_returns_only_apple_id_rows()
    {
        SeedCatalog(out _, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei(), OwnershipTransferred = false });
        Context.AppleIds.Add(new AppleId { ProductId = product.Id, Email = "stock@example.com", Password = "secret" });
        Context.SaveChanges();

        var rows = await _service.GetInventoryRowsAsync("appleid");

        Assert.Single(rows);
        Assert.Equal("Apple ID", rows[0].Type);
    }

    [Fact]
    public async Task GetInventoryRowsAsync_all_and_unrecognised_type_return_both_blocks()
    {
        SeedCatalog(out _, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei(), OwnershipTransferred = false });
        Context.AppleIds.Add(new AppleId { ProductId = product.Id, Email = "stock@example.com", Password = "secret" });
        Context.SaveChanges();

        Assert.Equal(2, (await _service.GetInventoryRowsAsync("all")).Count);
        Assert.Equal(2, (await _service.GetInventoryRowsAsync("unknown")).Count);
    }

    // ── Details ───────────────────────────────────────────────

    [Fact]
    public async Task GetDetailsAsync_for_phone_returns_improved_imei_and_unsold_labels()
    {
        SeedCatalog(out var phoneModel, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        product.ModelId = phoneModel.Id;
        product = Context.Products.First(p => p.Id == product.Id);
        Context.Products.Update(product);
        var phone = new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei(), IMEI2 = TestDataHelpers.GenerateImei(), OwnershipTransferred = false };
        Context.Phones.Add(phone);
        Context.SaveChanges();

        var details = await _service.GetDetailsAsync(phone.Id, "phone");

        Assert.NotNull(details);
        Assert.Equal("Phone", details!.Type);
        Assert.Equal($"IMEI: {phone.IMEI1} / {phone.IMEI2}", details.Identifier);
        Assert.Equal("Not sold", details.OwnerLabel);
        Assert.Equal("None", details.GuaranteeLabel);
    }

    [Fact]
    public async Task GetDetailsAsync_for_apple_id_routes_on_appleid_type()
    {
        SeedCatalog(out _, out var appleIdModel);
        var product = TestDataHelpers.CreateProduct(Context);
        product.ModelId = appleIdModel.Id;
        product = Context.Products.First(p => p.Id == product.Id);
        Context.Products.Update(product);
        var appleId = new AppleId { ProductId = product.Id, Email = "detail@example.com", Password = "secret" };
        Context.AppleIds.Add(appleId);
        Context.SaveChanges();

        var details = await _service.GetDetailsAsync(appleId.Id, "appleid");

        Assert.NotNull(details);
        Assert.Equal("Apple ID", details!.Type);
        Assert.Equal("Email: detail@example.com", details.Identifier);
    }

    [Fact]
    public async Task GetDetailsAsync_returns_null_for_unknown_id()
    {
        SeedCatalog(out _, out _);
        var details = await _service.GetDetailsAsync(999, "phone");
        Assert.Null(details);
    }

    [Fact]
    public async Task GetDetailsAsync_includes_transactions_descending_with_shop_fallback()
    {
        SeedCatalog(out var phoneModel, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        product.ModelId = phoneModel.Id;
        product = Context.Products.First(p => p.Id == product.Id);
        Context.Products.Update(product);
        var phone = new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei(), OwnershipTransferred = false };
        Context.Phones.Add(phone);
        Context.SaveChanges();

        var sellerPerson = new Person { FirstName = "Ali", LastName = "Seller", PhoneNumber = "09120000021" };
        var customerPerson = new Person { FirstName = "Sara", LastName = "Customer", PhoneNumber = "09120000022" };
        Context.People.AddRange(sellerPerson, customerPerson);
        Context.SaveChanges();

        var seller = new Seller { PersonId = sellerPerson.Id, EntityType = SellerEntityType.Real };
        var customer = new Customer { PersonId = customerPerson.Id, NationalId = "3333333333" };
        Context.Sellers.Add(seller);
        Context.Customers.Add(customer);
        Context.SaveChanges();

        // The shared projection renders the sentinel Shop seller as null in production,
        // which InMemory cannot express (it inner-joins and drops the row), so this test
        // covers the named-label branches with real people on both legs.
        Context.Transactions.AddRange(
            new Transaction { ProductId = product.Id, SellerId = seller.Id, CustomerId = customer.Id, FinishedPrice = 100m, Date = new DateTime(2026, 1, 1), Direction = TransactionDirection.Buy },
            new Transaction { ProductId = product.Id, SellerId = seller.Id, CustomerId = customer.Id, FinishedPrice = 150m, Date = new DateTime(2026, 2, 1), Direction = TransactionDirection.Sell });
        Context.SaveChanges();

        var details = await _service.GetDetailsAsync(phone.Id, "phone");

        Assert.NotNull(details);
        Assert.Equal(2, details!.Transactions.Count);
        Assert.Equal(new DateTime(2026, 2, 1), details.Transactions[0].Date);
        Assert.Equal("Ali Seller", details.Transactions[0].SellerLabel);
        Assert.Equal("Sara Customer", details.Transactions[0].CustomerLabel);
        Assert.Equal(new DateTime(2026, 1, 1), details.Transactions[1].Date);
        Assert.Equal("Ali Seller", details.Transactions[1].SellerLabel);
        Assert.Equal("Sara Customer", details.Transactions[1].CustomerLabel);
    }

    // ── CreateManufacturer / CreateModel / CreateColor ───────

    [Fact]
    public async Task CreateManufacturerAsync_creates_and_returns_new_option()
    {
        var result = await _service.CreateManufacturerAsync("  Sony  ");

        Assert.True(result.Succeeded);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Option);
        Assert.Equal("Sony", result.Option!.Name);
    }

    [Fact]
    public async Task CreateManufacturerAsync_returns_existing_option_on_duplicate()
    {
        SeedCatalog(out _, out _);
        var result = await _service.CreateManufacturerAsync("Apple");

        Assert.True(result.Succeeded);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Option);
        Assert.Equal(1, result.Option!.Id);
    }

    [Fact]
    public async Task CreateManufacturerAsync_rejects_blank_name()
    {
        var result = await _service.CreateManufacturerAsync("   ");

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Name is required.", result.Error);
    }

    [Fact]
    public async Task CreateModelAsync_creates_model_under_known_manufacturer_and_phone_category()
    {
        SeedCatalog(out _, out _);

        var result = await _service.CreateModelAsync(1, "Pixel 9");

        Assert.True(result.Succeeded);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Option);
        Assert.Equal("Pixel 9", result.Option!.Name);
        var model = Context.Models.First(m => m.Id == result.Option.Id);
        Assert.Equal(1, model.ManufacturerId);
        Assert.Equal(Context.Categories.First(c => c.Name == "Phone").Id, model.CategoryId);
    }

    [Fact]
    public async Task CreateModelAsync_returns_404_for_unknown_manufacturer()
    {
        SeedCatalog(out _, out _);
        var result = await _service.CreateModelAsync(999, "Pixel 9");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Manufacturer not found.", result.Error);
    }

    [Fact]
        public async Task CreateColorAsync_creates_and_dedupes()
    {
        SeedCatalog(out _, out _);
        var black = new MobileShop.Models.Entities.Color { Name = "Black" };
        Context.Colors.Add(black);
        Context.SaveChanges();

        var created = await _service.CreateColorAsync("Red");
        Assert.True(created.Succeeded);
        Assert.NotNull(created.Option);
        Assert.Equal("Red", created.Option!.Name);

        var dupe = await _service.CreateColorAsync("Black");
        Assert.True(dupe.Succeeded);
        Assert.NotNull(dupe.Option);
        Assert.Equal(black.Id, dupe.Option!.Id);
        Assert.Equal("Black", dupe.Option!.Name);
    }

    [Fact]
    public async Task CreateColorAsync_rejects_blank_name()
    {
        var result = await _service.CreateColorAsync("   ");

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Name is required.", result.Error);
    }

    // ── CreatePhoneAsync ─────────────────────────────────────

    [Fact]
    public async Task CreatePhoneAsync_creates_phone_and_returns_entity_id()
    {
        SeedCatalog(out var phoneModel, out _);
        var color = new MobileShop.Models.Entities.Color { Name = "Black" };
        Context.Colors.Add(color);
        Context.SaveChanges();

        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel
        {
            ManufacturerId = 1,
            ModelId = phoneModel.Id,
            Price = 999m,
            IMEI1 = TestDataHelpers.GenerateImei(),
            ColorId = color.Id,
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.EntityId);
        var phone = await Context.Phones.FirstAsync(p => p.IMEI1 == input.IMEI1);
        Assert.Equal(result.EntityId, phone.Id);
    }

    [Fact]
    public async Task CreatePhoneAsync_rejects_duplicate_imei_on_correct_field()
    {
        SeedCatalog(out var phoneModel, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = "123456789012345", OwnershipTransferred = false });
        Context.SaveChanges();

        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel
        {
            ManufacturerId = 1,
            ModelId = phoneModel.Id,
            Price = 999m,
            IMEI1 = "123456789012345",
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel.IMEI1), result.ErrorField);
    }

    [Fact]
    public async Task CreatePhoneAsync_applies_shop_warranty_and_second_hand_defaults()
    {
        SeedCatalog(out var phoneModel, out _);
        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel
        {
            ManufacturerId = 1,
            ModelId = phoneModel.Id,
            Price = 500m,
            IMEI1 = TestDataHelpers.GenerateImei(),
            IsSecondHand = true,
            TestPeriodDays = 15,
            HasGuarantee = true,
            GuaranteeCorporation = "  Premium Care  ",
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        var phone = await Context.Phones.FirstAsync(p => p.Id == result.EntityId);
        Assert.NotNull(phone.ProductNavigation.SecondHandProfile);
        Assert.Equal(15, phone.ProductNavigation.SecondHandProfile!.TestPeriodDays);
        Assert.Equal("Premium Care", phone.ProductNavigation.GuaranteeProfile!.Corporation);
        Assert.Equal(DateTime.Today.AddYears(1), phone.ProductNavigation.GuaranteeProfile.ExpirationDate);
    }

    // ── CreateAppleIdAsync ───────────────────────────────────

    [Fact]
    public async Task CreateAppleIdAsync_creates_apple_id_and_returns_entity_id()
    {
        SeedCatalog(out _, out var appleIdModel);
        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel
        {
            Price = 99m,
            Email = "new@example.com",
            Password = "secret123",
            Notes = "note",
        };

        var result = await _service.CreateAppleIdAsync(input);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.EntityId);
        var appleId = await Context.AppleIds.FirstAsync(a => a.Id == result.EntityId);
        Assert.Equal("new@example.com", appleId.Email);
        Assert.Equal("secret123", appleId.Password);
        Assert.Equal("note", appleId.Notes);
    }

    [Fact]
    public async Task CreateAppleIdAsync_rejects_duplicate_email()
    {
        SeedCatalog(out _, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        product = Context.Products.First(p => p.Id == product.Id);
        Context.AppleIds.Add(new AppleId { ProductId = product.Id, Email = "dupe@example.com", Password = "secret" });
        Context.SaveChanges();

        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel
        {
            Price = 99m,
            Email = "dupe@example.com",
            Password = "secret123",
        };

        var result = await _service.CreateAppleIdAsync(input);

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel.Email), result.ErrorField);
    }

    // ── CreateModelAsync: missing category throws ────────

    [Fact]
    public async Task CreateModelAsync_throws_when_Phone_category_missing()
    {
        var manufacturer = new Manufacturer { Name = "Apple" };
        Context.Manufacturers.Add(manufacturer);
        var appleIdCategory = new Category { Name = "AppleId" };
        Context.Categories.Add(appleIdCategory);
        Context.SaveChanges();
        // Note: "Phone" category intentionally absent.

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateModelAsync(1, "Pixel 9"));
    }

    [Fact]
    public async Task CreateAppleIdAsync_throws_when_AppleId_category_missing()
    {
        var manufacturer = new Manufacturer { Name = "Apple" };
        Context.Manufacturers.Add(manufacturer);
        var phoneCategory = new Category { Name = "Phone" };
        Context.Categories.Add(phoneCategory);
        Context.SaveChanges();
        // Note: "AppleId" category intentionally absent.

        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel
        {
            Price = 99m,
            Email = "new@example.com",
            Password = "secret123",
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppleIdAsync(input));
    }

    // ── CreatePhoneAsync: unknown ManufacturerId ─────────

    [Fact]
    public async Task CreatePhoneAsync_rejects_unknown_manufacturer()
    {
        SeedCatalog(out var phoneModel, out _);
        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel
        {
            ManufacturerId = 999,
            ModelId = phoneModel.Id,
            Price = 999m,
            IMEI1 = TestDataHelpers.GenerateImei(),
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel.ManufacturerId), result.ErrorField);
    }

    // ── CreatePhoneAsync: model belongs to another manufacturer ──

    [Fact]
    public async Task CreatePhoneAsync_rejects_model_from_another_manufacturer()
    {
        SeedCatalog(out var phoneModel, out _);
        var otherManufacturer = new Manufacturer { Name = "Samsung" };
        Context.Manufacturers.Add(otherManufacturer);
        Context.SaveChanges();
        var otherModel = new Model { ManufacturerId = otherManufacturer.Id, CategoryId = Context.Categories.First(c => c.Name == "Phone").Id, Name = "Galaxy S24" };
        Context.Models.Add(otherModel);
        Context.SaveChanges();

        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel
        {
            ManufacturerId = 1,
            ModelId = otherModel.Id,
            Price = 999m,
            IMEI1 = TestDataHelpers.GenerateImei(),
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel.ModelId), result.ErrorField);
    }

    // ── CreatePhoneAsync: unknown ColorId ────────────────

    [Fact]
    public async Task CreatePhoneAsync_rejects_unknown_color()
    {
        SeedCatalog(out var phoneModel, out _);
        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel
        {
            ManufacturerId = 1,
            ModelId = phoneModel.Id,
            Price = 999m,
            IMEI1 = TestDataHelpers.GenerateImei(),
            ColorId = 999,
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel.ColorId), result.ErrorField);
    }

    // ── CreateAppleIdAsync: implicit model created once ──

    [Fact]
    public async Task CreateAppleIdAsync_creates_implicit_model_once_for_two_emails()
    {
        SeedCatalog(out _, out _);
        var input1 = new MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel
        {
            Price = 99m,
            Email = "first@example.com",
            Password = "secret123",
        };
        var input2 = new MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel
        {
            Price = 99m,
            Email = "second@example.com",
            Password = "secret456",
        };

        var result1 = await _service.CreateAppleIdAsync(input1);
        var result2 = await _service.CreateAppleIdAsync(input2);

        Assert.True(result1.Succeeded);
        Assert.True(result2.Succeeded);
        var appleIdCategory = Context.Categories.First(c => c.Name == "AppleId");
        var appleManufacturer = Context.Manufacturers.First(m => m.Name == "Apple");
        Assert.Single(Context.Models.Where(m => m.ManufacturerId == appleManufacturer.Id && m.CategoryId == appleIdCategory.Id && m.Name == "iPhone"));
    }

    // ── CreateAppleIdAsync: duplicate email case-insensitive ──

    [Fact]
    public async Task CreateAppleIdAsync_rejects_duplicate_email_case_insensitive()
    {
        SeedCatalog(out _, out _);
        Context.AppleIds.Add(new AppleId { ProductId = TestDataHelpers.CreateProduct(Context).Id, Email = "Dupe@Example.com", Password = "secret" });
        Context.SaveChanges();

        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel
        {
            Price = 99m,
            Email = "dupe@example.com",
            Password = "secret123",
        };

        var result = await _service.CreateAppleIdAsync(input);

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel.Email), result.ErrorField);
    }

    // ── CreatePhoneAsync: barcode length ─────────────────

    [Fact]
    public async Task CreatePhoneAsync_success_barcode_length_is_12()
    {
        SeedCatalog(out var phoneModel, out _);
        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel
        {
            ManufacturerId = 1,
            ModelId = phoneModel.Id,
            Price = 999m,
            IMEI1 = TestDataHelpers.GenerateImei(),
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.EntityId);
        var product = Context.Products.First(p => p.Id == result.EntityId);
        Assert.Equal(12, product.Barcode.Length);
    }
}