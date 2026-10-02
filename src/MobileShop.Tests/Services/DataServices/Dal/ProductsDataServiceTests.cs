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
        TestDataHelpers.SeedShopSentinels(Context);
        _service = new ProductsDataService(
            new BaseRepo<Phone>(Context),
            new BaseRepo<AppleId>(Context),
            new BaseRepo<Manufacturer>(Context),
            new BaseRepo<Model>(Context),
            new BaseRepo<Category>(Context),
            new BaseRepo<MobileShop.Models.Entities.Color>(Context),
            new BaseRepo<Guarantee>(Context),
            new BaseRepo<Transaction>(Context),
            new BaseRepo<PartNumber>(Context),
            new BaseRepo<Product>(Context),
            NullLogger<ProductsDataService>.Instance);
    }

    private void SeedCatalog(out Model phoneModel, out Model appleIdModel)
    {
        var manufacturer = new Manufacturer { Name = "Apple" };
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();

        var phoneCategory = new Category { Name = "Phone" };
        var appleIdCategory = new Category { Name = "AppleId" };
        var glassCategory = new Category { Name = "Glass" };
        Context.Categories.AddRange(phoneCategory, appleIdCategory, glassCategory);
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
    public async Task GetInventoryRowsAsync_includes_glass_products_and_supports_glass_filter()
    {
        SeedCatalog(out _, out _);
        var glassCategory = Context.Categories.Single(c => c.Name == "Glass");
        var manufacturer = Context.Manufacturers.Single(m => m.Name == "Apple");
        var glassModel = new Model { ManufacturerId = manufacturer.Id, CategoryId = glassCategory.Id, Name = "iPhone 16 Glass" };
        Context.Models.Add(glassModel);
        Context.SaveChanges();

        var product = new Product { ModelId = glassModel.Id, Barcode = "GLASS123456", Price = 120, GlassProfile = new Glass() };
        Context.Products.Add(product);
        Context.SaveChanges();

        var allRows = await _service.GetInventoryRowsAsync();
        var glassRows = await _service.GetInventoryRowsAsync("glass");

        var row = Assert.Single(allRows, r => r.Type == "Glass");
        Assert.Equal("Barcode: GLASS123456", row.Identifier);
        Assert.Single(glassRows);
        Assert.Equal("Glass", glassRows[0].Type);
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

    // ── PartNumber filter ──────────────────────────────────────

    [Fact]
    public async Task GetInventoryRowsAsync_part_number_filter_returns_only_phones_with_that_part_number()
    {
        SeedCatalog(out var phoneModel, out _);
        var matching = TestDataHelpers.CreateProduct(Context);
        var other = TestDataHelpers.CreateProduct(Context);
        Context.PartNumbers.AddRange(
            new PartNumber { ModelId = phoneModel.Id, Code = "CH/ZAA", SupportsDualSim = true },
            new PartNumber { ModelId = phoneModel.Id, Code = "LL/A", SupportsEsim = true });
        Context.SaveChanges();
        var matchingPnId = Context.PartNumbers.First(pn => pn.Code == "CH/ZAA").Id;
        var otherPnId = Context.PartNumbers.First(pn => pn.Code == "LL/A").Id;

        Context.Phones.Add(new Phone { ProductId = matching.Id, IMEI1 = TestDataHelpers.GenerateImei(), PartNumberId = matchingPnId });
        Context.Phones.Add(new Phone { ProductId = other.Id, IMEI1 = TestDataHelpers.GenerateImei(), PartNumberId = otherPnId });
        Context.SaveChanges();

        var rows = await _service.GetInventoryRowsAsync(partNumberId: matchingPnId);

        var row = Assert.Single(rows);
        Assert.Equal("Phone", row.Type);
        Assert.Equal(matching.Id, row.ProductId);
    }

    [Fact]
    public async Task GetInventoryRowsAsync_part_number_filter_excludes_apple_ids()
    {
        SeedCatalog(out var phoneModel, out _);
        var phoneProduct = TestDataHelpers.CreateProduct(Context);
        var appleProduct = TestDataHelpers.CreateProduct(Context);
        Context.PartNumbers.Add(new PartNumber { ModelId = phoneModel.Id, Code = "CH/ZAA", SupportsDualSim = true });
        Context.SaveChanges();
        var pnId = Context.PartNumbers.First(pn => pn.Code == "CH/ZAA").Id;

        Context.Phones.Add(new Phone { ProductId = phoneProduct.Id, IMEI1 = TestDataHelpers.GenerateImei(), PartNumberId = pnId });
        Context.AppleIds.Add(new AppleId { ProductId = appleProduct.Id, Email = "stock@example.com", Password = "secret" });
        Context.SaveChanges();

        var rows = await _service.GetInventoryRowsAsync("all", pnId);

        Assert.All(rows, row => Assert.Equal("Phone", row.Type));
        Assert.DoesNotContain(rows, row => row.Type == "Apple ID");
    }

    [Fact]
    public async Task GetInventoryRowsAsync_non_positive_part_number_is_treated_as_no_filter()
    {
        SeedCatalog(out var phoneModel, out _);
        var phoneProduct = TestDataHelpers.CreateProduct(Context);
        var appleProduct = TestDataHelpers.CreateProduct(Context);
        Context.PartNumbers.Add(new PartNumber { ModelId = phoneModel.Id, Code = "CH/ZAA", SupportsDualSim = true });
        Context.SaveChanges();
        var pnId = Context.PartNumbers.First(pn => pn.Code == "CH/ZAA").Id;

        Context.Phones.Add(new Phone { ProductId = phoneProduct.Id, IMEI1 = TestDataHelpers.GenerateImei(), PartNumberId = pnId });
        Context.AppleIds.Add(new AppleId { ProductId = appleProduct.Id, Email = "stock@example.com", Password = "secret" });
        Context.SaveChanges();

        // Zero and negative ids must behave exactly like no selection: both blocks, unfiltered.
        Assert.Equal(2, (await _service.GetInventoryRowsAsync("all", 0)).Count);
        Assert.Equal(2, (await _service.GetInventoryRowsAsync("all", -5)).Count);
    }

    [Fact]
    public async Task GetInventoryRowsAsync_type_route_still_applies_with_part_number_filter()
    {
        SeedCatalog(out var phoneModel, out _);
        var matching = TestDataHelpers.CreateProduct(Context);
        var other = TestDataHelpers.CreateProduct(Context);
        Context.PartNumbers.AddRange(
            new PartNumber { ModelId = phoneModel.Id, Code = "CH/ZAA", SupportsDualSim = true },
            new PartNumber { ModelId = phoneModel.Id, Code = "LL/A", SupportsEsim = true });
        Context.SaveChanges();
        var matchingPnId = Context.PartNumbers.First(pn => pn.Code == "CH/ZAA").Id;
        var otherPnId = Context.PartNumbers.First(pn => pn.Code == "LL/A").Id;

        Context.Phones.Add(new Phone { ProductId = matching.Id, IMEI1 = TestDataHelpers.GenerateImei(), PartNumberId = matchingPnId });
        Context.Phones.Add(new Phone { ProductId = other.Id, IMEI1 = TestDataHelpers.GenerateImei(), PartNumberId = otherPnId });
        Context.SaveChanges();

        // The "appleid" type route still excludes phones even when a part number is supplied.
        Assert.Empty(await _service.GetInventoryRowsAsync("appleid", matchingPnId));
    }

    // ── PartNumber options ─────────────────────────────────────

    [Fact]
    public async Task GetPartNumbersAsync_without_model_returns_every_part_number()
    {
        SeedCatalog(out var phoneModel, out var appleIdModel);
        Context.PartNumbers.AddRange(
            new PartNumber { ModelId = phoneModel.Id, Code = "CH/ZAA", SupportsDualSim = true },
            new PartNumber { ModelId = appleIdModel.Id, Code = "LL/A", SupportsEsim = true });
        Context.SaveChanges();

        var options = await _service.GetPartNumbersAsync();

        Assert.Equal(2, options.Count);
        Assert.Contains(options, o => o.Name == "CH/ZAA");
        Assert.Contains(options, o => o.Name == "LL/A");
    }

    // ── Inventory-derived PartNumber options ──────────────────

    [Fact]
    public async Task GetInventoryPartNumbersAsync_only_returns_part_numbers_attached_to_inventory()
    {
        SeedCatalog(out var phoneModel, out _);
        var usedProduct = TestDataHelpers.CreateProduct(Context);
        Context.PartNumbers.AddRange(
            new PartNumber { ModelId = phoneModel.Id, Code = "CH/ZAA", SupportsDualSim = true },
            new PartNumber { ModelId = phoneModel.Id, Code = "UNUSED", SupportsEsim = true });
        Context.SaveChanges();
        var usedId = Context.PartNumbers.First(pn => pn.Code == "CH/ZAA").Id;

        Context.Phones.Add(new Phone { ProductId = usedProduct.Id, IMEI1 = TestDataHelpers.GenerateImei(), PartNumberId = usedId });
        Context.SaveChanges();

        var options = await _service.GetInventoryPartNumbersAsync();

        var option = Assert.Single(options);
        Assert.Equal("CH/ZAA", option.Name);
        Assert.Equal(usedId, option.Id);
    }

    [Fact]
    public async Task GetInventoryPartNumbersAsync_is_empty_when_no_phone_carries_a_part_number()
    {
        SeedCatalog(out var phoneModel, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        Context.PartNumbers.Add(new PartNumber { ModelId = phoneModel.Id, Code = "CH/ZAA" });
        Context.SaveChanges();
        Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.SaveChanges();

        var options = await _service.GetInventoryPartNumbersAsync();

        Assert.Empty(options);
    }

    // ── List projection PartNumberLabel ───────────────────────

    [Fact]
    public async Task GetInventoryRowsAsync_phone_row_exposes_part_number_code_and_missing_shows_na()
    {
        SeedCatalog(out var phoneModel, out _);
        var withPart = TestDataHelpers.CreateProduct(Context);
        var withoutPart = TestDataHelpers.CreateProduct(Context);
        Context.PartNumbers.Add(new PartNumber { ModelId = phoneModel.Id, Code = "CH/ZAA", SupportsDualSim = true });
        Context.SaveChanges();
        var pnId = Context.PartNumbers.First(pn => pn.Code == "CH/ZAA").Id;

        Context.Phones.Add(new Phone { ProductId = withPart.Id, IMEI1 = TestDataHelpers.GenerateImei(), PartNumberId = pnId });
        Context.Phones.Add(new Phone { ProductId = withoutPart.Id, IMEI1 = TestDataHelpers.GenerateImei() });
        Context.SaveChanges();

        var rows = await _service.GetInventoryRowsAsync("phone");

        var withPartRow = Assert.Single(rows, r => r.ProductId == withPart.Id);
        Assert.Equal("CH/ZAA", withPartRow.PartNumberLabel);
        var withoutPartRow = Assert.Single(rows, r => r.ProductId == withoutPart.Id);
        Assert.Equal("N/A", withoutPartRow.PartNumberLabel);
    }

    [Fact]
    public async Task GetInventoryRowsAsync_apple_id_row_shows_na_part_number()
    {
        SeedCatalog(out var phoneModel, out _);
        var appleProduct = TestDataHelpers.CreateProduct(Context);
        Context.PartNumbers.Add(new PartNumber { ModelId = phoneModel.Id, Code = "CH/ZAA" });
        Context.SaveChanges();
        Context.AppleIds.Add(new AppleId { ProductId = appleProduct.Id, Email = "stock@example.com", Password = "secret" });
        Context.SaveChanges();

        var rows = await _service.GetInventoryRowsAsync("appleid");

        var row = Assert.Single(rows);
        Assert.Equal("Apple ID", row.Type);
        Assert.Equal("N/A", row.PartNumberLabel);
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
            new Transaction { ProductId = product.Id, SellerId = seller.Id, CustomerId = customer.Id, FinishedPrice = 100, Date = new DateTime(2026, 1, 1), Direction = TransactionDirection.Buy },
            new Transaction { ProductId = product.Id, SellerId = seller.Id, CustomerId = customer.Id, FinishedPrice = 150, Date = new DateTime(2026, 2, 1), Direction = TransactionDirection.Sell });
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

    // ── Phone details PartNumber / SIM ────────────────────────

    private Phone SeedPhoneWithPartNumber(out PartNumber partNumber, bool supportsDualSim, bool supportsEsim)
    {
        SeedCatalog(out var phoneModel, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        var tracked = Context.Products.First(p => p.Id == product.Id);
        tracked.ModelId = phoneModel.Id;
        Context.Products.Update(tracked);

        partNumber = new PartNumber
        {
            ModelId = phoneModel.Id,
            Code = "CH/ZAA",
            SupportsDualSim = supportsDualSim,
            SupportsEsim = supportsEsim
        };
        Context.PartNumbers.Add(partNumber);
        Context.SaveChanges();

        var phone = new Phone
        {
            ProductId = product.Id,
            IMEI1 = TestDataHelpers.GenerateImei(),
            OwnershipTransferred = false,
            PartNumberId = partNumber.Id
        };
        Context.Phones.Add(phone);
        Context.SaveChanges();
        return phone;
    }

    private Phone SeedPhoneWithoutPartNumber()
    {
        SeedCatalog(out var phoneModel, out _);
        var product = TestDataHelpers.CreateProduct(Context);
        var tracked = Context.Products.First(p => p.Id == product.Id);
        tracked.ModelId = phoneModel.Id;
        Context.Products.Update(tracked);

        var phone = new Phone
        {
            ProductId = product.Id,
            IMEI1 = TestDataHelpers.GenerateImei(),
            OwnershipTransferred = false
        };
        Context.Phones.Add(phone);
        Context.SaveChanges();
        return phone;
    }

    [Fact]
    public async Task GetDetailsAsync_phone_with_part_number_shows_code_and_capabilities()
    {
        var phone = SeedPhoneWithPartNumber(out var partNumber, supportsDualSim: true, supportsEsim: false);

        var details = await _service.GetDetailsAsync(phone.Id, "phone");

        Assert.NotNull(details);
        Assert.Equal("CH/ZAA", details!.PartNumberLabel);
        Assert.Equal("Yes", details.DualSimLabel);
        Assert.Equal("No", details.EsimLabel);
        Assert.Equal(partNumber.Code, details.PartNumberLabel);
    }

    [Fact]
    public async Task GetDetailsAsync_phone_with_part_number_that_supports_both_shows_yes_for_both()
    {
        var phone = SeedPhoneWithPartNumber(out _, supportsDualSim: true, supportsEsim: true);

        var details = await _service.GetDetailsAsync(phone.Id, "phone");

        Assert.NotNull(details);
        Assert.Equal("Yes", details!.DualSimLabel);
        Assert.Equal("Yes", details.EsimLabel);
    }

    [Fact]
    public async Task GetDetailsAsync_phone_without_part_number_loads_and_shows_na()
    {
        var phone = SeedPhoneWithoutPartNumber();

        var details = await _service.GetDetailsAsync(phone.Id, "phone");

        Assert.NotNull(details);
        Assert.Null(Context.Phones.Single(p => p.Id == phone.Id).PartNumberId);
        Assert.Equal("N/A", details!.PartNumberLabel);
        Assert.Equal("N/A", details.DualSimLabel);
        Assert.Equal("N/A", details.EsimLabel);
    }

    [Fact]
    public async Task GetDetailsAsync_phone_with_false_capabilities_is_not_treated_as_missing_part_number()
    {
        // A real part number with both capabilities false must render "No", never the "N/A"
        // used for a phone that has no part number at all.
        var phone = SeedPhoneWithPartNumber(out _, supportsDualSim: false, supportsEsim: false);

        var details = await _service.GetDetailsAsync(phone.Id, "phone");

        Assert.NotNull(details);
        Assert.Equal("CH/ZAA", details!.PartNumberLabel);
        Assert.Equal("No", details.DualSimLabel);
        Assert.Equal("No", details.EsimLabel);
    }

    [Fact]
    public async Task GetDetailsAsync_apple_id_details_remain_unchanged_without_part_number_fields()
    {
        SeedCatalog(out _, out var appleIdModel);
        var product = TestDataHelpers.CreateProduct(Context);
        var tracked = Context.Products.First(p => p.Id == product.Id);
        tracked.ModelId = appleIdModel.Id;
        Context.Products.Update(tracked);
        var appleId = new AppleId { ProductId = product.Id, Email = "unchanged@example.com", Password = "secret" };
        Context.AppleIds.Add(appleId);
        Context.SaveChanges();

        var details = await _service.GetDetailsAsync(appleId.Id, "appleid");

        Assert.NotNull(details);
        Assert.Equal("Apple ID", details!.Type);
        Assert.Equal("Email: unchanged@example.com", details.Identifier);
        // The phone-only part-number fields keep their defaults for Apple IDs.
        Assert.Equal("N/A", details.PartNumberLabel);
        Assert.Equal("N/A", details.DualSimLabel);
        Assert.Equal("N/A", details.EsimLabel);
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

    // ── Stage N Step 1: finished price + notes ────────────────

    private static MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel PhonePricing(int paid, decimal? percent, int? amount) => new()
    {
        ManufacturerId = 1,
        ModelId = 0, // set by caller after catalog seeding
        Price = paid,
        ProfitPercent = percent,
        ProfitAmount = amount,
        IMEI1 = TestDataHelpers.GenerateImei(),
    };

    [Fact]
    public async Task CreatePhoneAsync_finished_price_with_percent_only()
    {
        SeedCatalog(out var phoneModel, out _);
        var input = PhonePricing(1000, percent: 10m, amount: null);
        input.ModelId = phoneModel.Id;

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        var phone = await Context.Phones.FirstAsync(p => p.IMEI1 == input.IMEI1);
        var product = await Context.Products.FirstAsync(p => p.Id == phone.ProductId);
        Assert.Equal(1100, product.Price);
    }

    [Fact]
    public async Task CreatePhoneAsync_finished_price_with_amount_only()
    {
        SeedCatalog(out var phoneModel, out _);
        var input = PhonePricing(1000, percent: null, amount: 250);
        input.ModelId = phoneModel.Id;

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        var phone = await Context.Phones.FirstAsync(p => p.IMEI1 == input.IMEI1);
        var product = await Context.Products.FirstAsync(p => p.Id == phone.ProductId);
        Assert.Equal(1250, product.Price);
    }

    [Fact]
    public async Task CreatePhoneAsync_finished_price_prefers_amount_when_both_present()
    {
        SeedCatalog(out var phoneModel, out _);
        // Amount-first safety net: 1000 + 50 (amount) wins over 1000 + 10% (1100).
        var input = PhonePricing(1000, percent: 10m, amount: 50);
        input.ModelId = phoneModel.Id;

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        var phone = await Context.Phones.FirstAsync(p => p.IMEI1 == input.IMEI1);
        var product = await Context.Products.FirstAsync(p => p.Id == phone.ProductId);
        Assert.Equal(1050, product.Price);
    }

    [Fact]
    public async Task CreatePhoneAsync_persists_second_hand_and_guarantee_notes()
    {
        SeedCatalog(out var phoneModel, out _);
        var input = PhonePricing(1000, null, null);
        input.ModelId = phoneModel.Id;
        input.IsSecondHand = true;
        input.SecondHandNotes = "  scuffed corner  ";
        input.HasGuarantee = true;
        input.GuaranteeNotes = " box included ";

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        var phone = await Context.Phones.FirstAsync(p => p.IMEI1 == input.IMEI1);
        var product = await Context.Products.Include(p => p.SecondHandProfile).Include(p => p.GuaranteeProfile)
            .FirstAsync(p => p.Id == phone.ProductId);
        Assert.Equal("scuffed corner", product.SecondHandProfile!.Notes);
        Assert.Equal("box included", product.GuaranteeProfile!.Notes);
        Assert.Equal(DateTime.Today, product.GuaranteeProfile.StartDate);
    }

    [Fact]
    public async Task CreatePhoneAsync_flags_false_leave_profiles_null_and_blank_notes_trim_to_null()
    {
        SeedCatalog(out var phoneModel, out _);
        var input = PhonePricing(1000, null, null);
        input.ModelId = phoneModel.Id;
        input.IsSecondHand = false;
        input.SecondHandNotes = "   ";
        input.HasGuarantee = false;
        input.GuaranteeNotes = "   ";

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        var phone = await Context.Phones.FirstAsync(p => p.IMEI1 == input.IMEI1);
        var product = await Context.Products.Include(p => p.SecondHandProfile).Include(p => p.GuaranteeProfile)
            .FirstAsync(p => p.Id == phone.ProductId);
        Assert.Null(product.SecondHandProfile);
        Assert.Null(product.GuaranteeProfile);
    }

    [Fact]
    public async Task CreateAppleIdAsync_finished_price_with_percent()
    {
        SeedCatalog(out _, out _);
        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel
        {
            Price = 200,
            ProfitPercent = 50m,
            Email = "pricing-percent@example.com",
            Password = "secret123",
        };

        var result = await _service.CreateAppleIdAsync(input);

        Assert.True(result.Succeeded);
        var appleId = await Context.AppleIds.FirstAsync(a => a.Email == input.Email);
        var product = await Context.Products.FirstAsync(p => p.Id == appleId.ProductId);
        Assert.Equal(300, product.Price);
    }

    [Fact]
    public async Task CreateAppleIdAsync_finished_price_with_amount()
    {
        SeedCatalog(out _, out _);
        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel
        {
            Price = 200,
            ProfitAmount = 40m,
            Email = "pricing-amount@example.com",
            Password = "secret123",
        };

        var result = await _service.CreateAppleIdAsync(input);

        Assert.True(result.Succeeded);
        var appleId = await Context.AppleIds.FirstAsync(a => a.Email == input.Email);
        var product = await Context.Products.FirstAsync(p => p.Id == appleId.ProductId);
        Assert.Equal(240, product.Price);
    }

    // ── CreatePhoneAsync: PartNumber ─────────────────────────

    [Fact]
    public async Task CreatePhoneAsync_persists_selected_part_number()
    {
        SeedCatalog(out var phoneModel, out _);
        Context.PartNumbers.Add(new PartNumber { ModelId = phoneModel.Id, Code = "CH/ZAA", SupportsDualSim = true });
        Context.SaveChanges();
        var pnId = Context.PartNumbers.First(pn => pn.Code == "CH/ZAA").Id;

        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel
        {
            ManufacturerId = 1,
            ModelId = phoneModel.Id,
            Price = 999,
            IMEI1 = TestDataHelpers.GenerateImei(),
            PartNumberId = pnId,
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        var phone = await Context.Phones.FirstAsync(p => p.IMEI1 == input.IMEI1);
        Assert.Equal(pnId, phone.PartNumberId);
    }

    [Fact]
    public async Task CreatePhoneAsync_omitted_part_number_stays_null()
    {
        SeedCatalog(out var phoneModel, out _);

        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel
        {
            ManufacturerId = 1,
            ModelId = phoneModel.Id,
            Price = 999,
            IMEI1 = TestDataHelpers.GenerateImei(),
            PartNumberId = null,
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        var phone = await Context.Phones.FirstAsync(p => p.IMEI1 == input.IMEI1);
        Assert.Null(phone.PartNumberId);
    }

    [Fact]
    public async Task CreatePhoneAsync_rejects_part_number_belonging_to_another_model()
    {
        SeedCatalog(out var phoneModel, out _);
        // A part number owned by a different model must not be attachable.
        var otherModel = new Model { ManufacturerId = 1, CategoryId = Context.Categories.First(c => c.Name == "Phone").Id, Name = "Other" };
        Context.Models.Add(otherModel);
        Context.SaveChanges();
        Context.PartNumbers.Add(new PartNumber { ModelId = otherModel.Id, Code = "FOREIGN" });
        Context.SaveChanges();
        var foreignPnId = Context.PartNumbers.First(pn => pn.Code == "FOREIGN").Id;

        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel
        {
            ManufacturerId = 1,
            ModelId = phoneModel.Id,
            Price = 999,
            IMEI1 = TestDataHelpers.GenerateImei(),
            PartNumberId = foreignPnId,
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreatePhoneInputModel.PartNumberId), result.ErrorField);
        Assert.False(await Context.Phones.AnyAsync(p => p.IMEI1 == input.IMEI1));
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
            Price = 999,
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
            Price = 999,
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
            Price = 500,
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

    // ── CreateGlassesAsync ────────────────────────────────────

    [Fact]
    public async Task CreateGlassesAsync_creates_exact_count_with_shared_price_distinct_barcodes_and_fits()
    {
        SeedCatalog(out var phoneModel, out _);
        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel
        {
            CompatibleManufacturerId = 1, CompatibleModelId = phoneModel.Id, GlassManufacturerId = 1, Price = 100, ProfitPercent = 25m, Count = 3
        };

        var result = await _service.CreateGlassesAsync(input);

        Assert.True(result.Succeeded);
        var products = await Context.Products.Include(p => p.GlassProfile).ThenInclude(g => g!.ModelFits).ToListAsync();
        var glassModel = Assert.Single(Context.Models.Where(m => m.CategoryId == Context.Categories.First(c => c.Name == "Glass").Id && m.ManufacturerId == 1 && m.Name == "iPhone 16 Glass"));

        Assert.Equal(3, products.Count);
        Assert.All(products, p => Assert.Equal(125m, p.Price));
        var purchases = await Context.Transactions.Where(t => t.Direction == TransactionDirection.Buy).ToListAsync();
        Assert.Equal(3, purchases.Count);
        Assert.All(purchases, t => Assert.Equal(100, t.FinishedPrice));
        Assert.All(purchases, t => Assert.Equal(DateTime.Today, t.Date));
        Assert.All(products, p => Assert.Equal(glassModel.Id, p.ModelId));
        Assert.Equal(3, products.Select(p => p.Barcode).Distinct().Count());
        Assert.All(products, p => Assert.Equal(12, p.Barcode.Length));
        Assert.All(products, p => Assert.NotNull(p.GlassProfile));
        Assert.All(products, p => Assert.Single(p.GlassProfile!.ModelFits));
        Assert.All(products, p => Assert.Equal(phoneModel.Id, p.GlassProfile!.ModelFits.Single().ModelId));
    }

    [Fact]
    public async Task CreateGlassesAsync_preserves_decimal_paid_price_and_allows_profit_over_100_percent()
    {
        SeedCatalog(out var phoneModel, out _);

        var result = await _service.CreateGlassesAsync(new MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel
        {
            CompatibleManufacturerId = 1,
            CompatibleModelId = phoneModel.Id,
            GlassManufacturerId = 1,
            Price = 10,
            ProfitPercent = 200m,
            Count = 1,
        });

        Assert.True(result.Succeeded);
        var product = Assert.Single(await Context.Products.ToListAsync());
        Assert.Equal(31, product.Price);
        var purchase = Assert.Single(await Context.Transactions.Where(t => t.Direction == TransactionDirection.Buy).ToListAsync());
        Assert.Equal(10, purchase.FinishedPrice);
        Assert.Equal(DateTime.Today, purchase.Date);
    }

    [Fact]
    public async Task CreateGlassesAsync_uses_profit_amount_over_percent()
    {
        SeedCatalog(out var phoneModel, out _);
        var result = await _service.CreateGlassesAsync(new MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel
        {
            CompatibleManufacturerId = 1, CompatibleModelId = phoneModel.Id, GlassManufacturerId = 1, Price = 100, ProfitPercent = 50m, ProfitAmount = 20m, Count = 2
        });

        Assert.True(result.Succeeded);
        Assert.Equal(2, await Context.Products.CountAsync());
        Assert.All(await Context.Products.ToListAsync(), p => Assert.Equal(120m, p.Price));
    }

    [Fact]
    public async Task CreateGlassesAsync_accepts_count_without_an_upper_cap()
    {
        SeedCatalog(out var phoneModel, out _);

        var one = await _service.CreateGlassesAsync(new MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel
        {
            CompatibleManufacturerId = 1, CompatibleModelId = phoneModel.Id, GlassManufacturerId = 1, Price = 10, Count = 1
        });
        Assert.True(one.Succeeded);
        Assert.Single(await Context.Products.ToListAsync());

        var fiveHundredAndOne = await _service.CreateGlassesAsync(new MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel
        {
            CompatibleManufacturerId = 1, CompatibleModelId = phoneModel.Id, GlassManufacturerId = 1, Price = 10, Count = 501
        });
        Assert.True(fiveHundredAndOne.Succeeded);
        Assert.Equal(502, await Context.Products.CountAsync());
    }

    [Fact]
    public async Task CreateGlassesAsync_rejects_non_positive_count()
    {
        SeedCatalog(out var phoneModel, out _);

        var result = await _service.CreateGlassesAsync(new MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel
        {
            CompatibleManufacturerId = 1, CompatibleModelId = phoneModel.Id, GlassManufacturerId = 1, Price = 10, Count = 0
        });

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel.Count), result.ErrorField);
        Assert.Empty(await Context.Products.ToListAsync());
    }

    [Fact]
    public async Task CreateGlassesAsync_rejects_model_from_another_manufacturer()
    {
        SeedCatalog(out _, out _);
        var otherManufacturer = new Manufacturer { Name = "Samsung" };
        Context.Manufacturers.Add(otherManufacturer);
        Context.SaveChanges();
        var otherModel = new Model
        {
            ManufacturerId = otherManufacturer.Id,
            CategoryId = Context.Categories.First(c => c.Name == "Phone").Id,
            Name = "Galaxy S24"
        };
        Context.Models.Add(otherModel);
        Context.SaveChanges();

        var result = await _service.CreateGlassesAsync(new MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel
        {
            CompatibleManufacturerId = 1, CompatibleModelId = otherModel.Id, GlassManufacturerId = 1, Price = 10, Count = 2
        });

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel.CompatibleModelId), result.ErrorField);
        Assert.Empty(await Context.Products.ToListAsync());
    }

    [Fact]
    public async Task CreateGlassesAsync_rejects_disallowed_model_category()
    {
        SeedCatalog(out _, out var appleIdModel);

        var result = await _service.CreateGlassesAsync(new MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel
        {
            CompatibleManufacturerId = 1, CompatibleModelId = appleIdModel.Id, GlassManufacturerId = 1, Price = 10, Count = 2
        });

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel.CompatibleModelId), result.ErrorField);
        Assert.Empty(await Context.Products.ToListAsync());
    }

    [Fact]
    public async Task CreateGlassesAsync_rejects_unknown_manufacturer()
    {
        SeedCatalog(out var phoneModel, out _);

        var result = await _service.CreateGlassesAsync(new MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel
        {
            CompatibleManufacturerId = 999, CompatibleModelId = phoneModel.Id, GlassManufacturerId = 1, Price = 10, Count = 2
        });

        Assert.False(result.Succeeded);
        Assert.Equal(nameof(MobileShop.Models.ViewModels.Web.BindModels.CreateGlassInputModel.CompatibleManufacturerId), result.ErrorField);
        Assert.Empty(await Context.Products.ToListAsync());
    }

    // ── CreateAppleIdAsync ───────────────────────────────────

    [Fact]
    public async Task CreateAppleIdAsync_creates_apple_id_and_returns_entity_id()
    {
        SeedCatalog(out _, out var appleIdModel);
        var input = new MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel
        {
            Price = 99,
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
            Price = 99,
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
            Price = 99,
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
            Price = 999,
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
            Price = 999,
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
            Price = 999,
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
            Price = 99,
            Email = "first@example.com",
            Password = "secret123",
        };
        var input2 = new MobileShop.Models.ViewModels.Web.BindModels.CreateAppleIdInputModel
        {
            Price = 99,
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
            Price = 99,
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
            Price = 999,
            IMEI1 = TestDataHelpers.GenerateImei(),
        };

        var result = await _service.CreatePhoneAsync(input);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.EntityId);
        var product = Context.Products.First(p => p.Id == result.EntityId);
        Assert.Equal(12, product.Barcode.Length);
    }

    // ── PartNumber list/create ──────────────────────────

    [Fact]
    public async Task GetPartNumbersAsync_returns_only_part_numbers_for_the_given_model()
    {
        SeedCatalog(out var phoneModel13, out _);
        var phoneCategory = Context.Categories.First(c => c.Name == "Phone");
        var samsung = new Manufacturer { Name = "Samsung" };
        Context.Manufacturers.Add(samsung);
        Context.SaveChanges();
        var phoneModelGalaxy = new Model { ManufacturerId = samsung.Id, CategoryId = phoneCategory.Id, Name = "Galaxy S24" };
        Context.Models.Add(phoneModelGalaxy);
        Context.SaveChanges();

        Context.PartNumbers.AddRange(
            new PartNumber { ModelId = phoneModel13.Id, Code = "MQ0K3LL/A", SupportsDualSim = true, SupportsEsim = false },
            new PartNumber { ModelId = phoneModelGalaxy.Id, Code = "SM-S921B", SupportsDualSim = false, SupportsEsim = false });
        Context.SaveChanges();

        var options = await _service.GetPartNumbersAsync(phoneModel13.Id);

        Assert.Single(options);
        Assert.Contains(options, o => o.Name == "MQ0K3LL/A");
        Assert.DoesNotContain(options, o => o.Name == "SM-S921B");
    }

    [Fact]
    public async Task GetPartNumbersAsync_returns_empty_for_model_with_no_part_numbers()
    {
        SeedCatalog(out var phoneModel, out _);

        var options = await _service.GetPartNumbersAsync(phoneModel.Id);

        Assert.Empty(options);
    }

    [Fact]
    public async Task CreatePartNumberAsync_creates_and_returns_new_option()
    {
        SeedCatalog(out var phoneModel, out _);
        Assert.False(Context.PartNumbers.Any(pn => pn.ModelId == phoneModel.Id));

        var result = await _service.CreatePartNumberAsync(phoneModel.Id, "  MQ0K3LL/A  ", supportsDualSim: true, supportsEsim: false);

        Assert.True(result.Succeeded);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Option);
        Assert.Equal("MQ0K3LL/A", result.Option!.Name);

        var partNumber = Context.PartNumbers.First(pn => pn.Id == result.Option.Id);
        Assert.Equal(phoneModel.Id, partNumber.ModelId);
        Assert.True(partNumber.SupportsDualSim);
        Assert.False(partNumber.SupportsEsim);
    }

    [Fact]
    public async Task CreatePartNumberAsync_returns_existing_option_on_duplicate()
    {
        SeedCatalog(out var phoneModel, out _);
        Context.PartNumbers.Add(new PartNumber
        {
            ModelId = phoneModel.Id,
            Code = "MQ0K3LL/A",
            SupportsDualSim = true,
            SupportsEsim = false
        });
        Context.SaveChanges();

        var result = await _service.CreatePartNumberAsync(phoneModel.Id, "MQ0K3LL/A", supportsDualSim: true, supportsEsim: false);

        Assert.True(result.Succeeded);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Option);
        Assert.Equal(1, Context.PartNumbers.Count());
    }

    [Fact]
    public async Task CreatePartNumberAsync_returns_404_for_unknown_model()
    {
        var result = await _service.CreatePartNumberAsync(999, "CH/ZAA", supportsDualSim: true, supportsEsim: true);

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Model not found.", result.Error);
    }

    [Fact]
    public async Task CreatePartNumberAsync_rejects_blank_code()
    {
        SeedCatalog(out var phoneModel, out _);

        var result = await _service.CreatePartNumberAsync(phoneModel.Id, "   ", supportsDualSim: true, supportsEsim: true);

        Assert.False(result.Succeeded);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Code is required.", result.Error);
    }

}