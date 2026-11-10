namespace MobileShop.Tests.Web.Pages.Products;

[Collection("MobileShop-Dal-Serial")]
public class EditModelTests : RepoTestBase
{
    private readonly ProductsDataService _service;

    public EditModelTests()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        _service = new ProductsDataService(
            new BaseRepo<Phone>(Context),
            new BaseRepo<AppleId>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Customer>(Context),
            new BaseRepo<Person>(Context),
            new BaseRepo<Manufacturer>(Context),
            new BaseRepo<Model>(Context),
            new BaseRepo<Category>(Context),
            new BaseRepo<MobileShop.Models.Entities.Color>(Context),
            new BaseRepo<Guarantee>(Context),
            new BaseRepo<Transaction>(Context),
            new BaseRepo<PartNumber>(Context),
            new BaseRepo<Product>(Context),
            new BaseRepo<StorageCapacity>(Context),
            new BaseRepo<Cpu>(Context),
            new BaseRepo<Gpu>(Context),
            NullLogger<ProductsDataService>.Instance);
    }

    [Fact]
    public async Task Get_edit_model_maps_paid_price_and_profit_from_purchase_transaction()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        product.Price = 7000;
        await Context.SaveChangesAsync();
        Context.Transactions.Add(new Transaction
        {
            ProductId = product.Id,
            SellerId = 1,
            CustomerId = 1,
            FinishedPrice = 5000,
            Date = new DateTime(2026, 1, 1),
            Direction = TransactionDirection.Buy,
        });
        await Context.SaveChangesAsync();
        var page = new EditModel(_service);

        var result = await page.OnGetAsync(product.Id);

        Assert.IsType<PageResult>(result);
        Assert.Equal(5000, page.Product.Price);
        Assert.Equal(2000, page.Product.ProfitAmount);
        Assert.Equal(40m, page.Product.ProfitPercent);
    }

    [Fact]
    public async Task Cable_post_rejects_missing_connector_and_repopulates_connector_options()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Cables.Add(new Cable { ProductId = product.Id, Connector1 = CableConnector.UsbC, Connector2 = CableConnector.UsbA, Length = 1.5m });
        await Context.SaveChangesAsync();
        var page = new EditModel(_service)
        {
            Product = new EditProductInputModel
            {
                ProductId = product.Id,
                Type = "Cable",
                Identifier = product.Barcode,
                Price = product.Price,
                Connector1 = null,
                Connector2 = CableConnector.UsbA,
            }
        };

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(page.ModelState.ContainsKey("Product.Connector1"));
        Assert.Contains(page.CableConnectors, option => option.Id == (int)CableConnector.UsbC);
        Assert.Contains(page.CableConnectors, option => option.Id == (int)CableConnector.Lightning);
        var unchanged = await Context.Cables.AsNoTracking().SingleAsync(item => item.ProductId == product.Id);
        Assert.Equal(CableConnector.UsbC, unchanged.Connector1);
    }

    [Fact]
    public async Task Case_edit_populates_phone_model_options_and_uses_model_ids_for_selections()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var phoneCategory = new Category { Name = "Phone" };
        var phoneManufacturer = new Manufacturer { Name = $"Phone manufacturer {Guid.NewGuid():N}" };
        Context.Categories.Add(phoneCategory);
        Context.Manufacturers.Add(phoneManufacturer);
        await Context.SaveChangesAsync();

        var phoneModel = new Model
        {
            ManufacturerId = phoneManufacturer.Id,
            CategoryId = phoneCategory.Id,
            Name = "Compatible phone model"
        };
        Context.Models.Add(phoneModel);
        await Context.SaveChangesAsync();

        Context.Cases.Add(new Case { ProductId = product.Id, ModelFits = [new CaseModelFit { ModelId = phoneModel.Id, ModelNavigation = phoneModel }] });
        await Context.SaveChangesAsync();
        var page = new EditModel(_service);
        var result = await page.OnGetAsync(product.Id);
        Assert.IsType<PageResult>(result);
        Assert.Contains(page.CompatibleModelOptions, option => option.Id == phoneModel.Id);
        Assert.Contains(phoneModel.Id, page.Product.CompatibleModels);
    }

    [Theory]
    [InlineData("Phone")]
    [InlineData("Apple ID")]
    [InlineData("Tablet")]
    [InlineData("Smart Watch")]
    [InlineData("Laptop")]
    [InlineData("Cable")]
    [InlineData("Charger")]
    [InlineData("Power Bank")]
    [InlineData("Portable Storage")]
    [InlineData("Case")]
    [InlineData("Glass")]
    public async Task Edit_get_maps_each_supported_product_type(string type)
    {
        var (product, compatibleModelId) = await CreateProductForTypeAsync(type);
        Context.ChangeTracker.Clear();

        var page = new EditModel(_service);
        var result = await page.OnGetAsync(product.Id);

        Assert.IsType<PageResult>(result);
        Assert.Equal(type, page.Product.Type);
        Assert.Equal(product.Id, page.Product.ProductId);
        Assert.Equal(product.Barcode, page.Product.Identifier);
        Assert.Equal("original note", page.Product.Notes);

        switch (type)
        {
            case "Phone":
                Assert.Equal("123456789012345", page.Product.IMEI1);
                break;
            case "Apple ID":
                Assert.Equal("original plaintext password", page.Product.AppleIdPassword);
                Assert.Equal("original answer", page.Product.SecurityAnswer1);
                break;
            case "Cable":
                Assert.Equal(CableConnector.UsbC, page.Product.Connector1);
                Assert.Equal(CableConnector.UsbA, page.Product.Connector2);
                break;
            case "Power Bank":
                Assert.Equal(1, page.Product.PortCount);
                Assert.Equal([(int)CableConnector.UsbC], page.Product.PortTypes);
                break;
            case "Portable Storage":
                Assert.Equal(StorageKind.UsbFlash, page.Product.StorageKind);
                Assert.Equal("128 GB", page.Product.StorageCapacityLabel);
                break;
            case "Case":
            case "Glass":
                Assert.Contains(compatibleModelId!.Value, page.Product.CompatibleModels);
                Assert.Contains(page.CompatibleModelOptions, option => option.Id == compatibleModelId.Value);
                if (type == "Glass")
                    Assert.Contains(page.SelectedGlassFits, option => option.Id == compatibleModelId.Value);
                break;
            case "Laptop":
                Assert.Equal(13.3m, page.Product.DisplaySize);
                break;
        }
    }

    [Theory]
    [InlineData("Phone")]
    [InlineData("Apple ID")]
    [InlineData("Tablet")]
    [InlineData("Smart Watch")]
    [InlineData("Laptop")]
    [InlineData("Cable")]
    [InlineData("Charger")]
    [InlineData("Power Bank")]
    [InlineData("Portable Storage")]
    [InlineData("Case")]
    [InlineData("Glass")]
    public async Task UpdateProductAsync_updates_profile_notes_for_each_supported_type(string type)
    {
        var (product, compatibleModelId) = await CreateProductForTypeAsync(type);
        var input = new EditProductInputModel
        {
            ProductId = product.Id,
            Type = type,
            Identifier = product.Barcode,
            Price = product.Price,
            Notes = "updated note"
        };

        switch (type)
        {
            case "Cable":
                input.Connector1 = CableConnector.UsbC;
                input.Connector2 = CableConnector.UsbA;
                input.CableLength = 1.5m;
                break;
            case "Charger":
                input.Wattage = 20;
                input.Pd = true;
                input.PortCount = 1;
                break;
            case "Power Bank":
                input.CapacityMah = 10000;
                input.MaxWattage = 20;
                input.PortCount = 1;
                input.PortTypes = [(int)CableConnector.UsbC];
                input.Pd = true;
                break;
            case "Portable Storage":
                input.StorageKind = StorageKind.UsbFlash;
                input.StorageCapacityId = product.PortableStorageProfile!.StorageCapacityId;
                input.Speed = 100;
                break;
            case "Laptop":
                input.DisplaySize = 13.3m;
                break;
            case "Case":
            case "Glass":
                input.CompatibleModels = [compatibleModelId!.Value];
                break;
        }

        var result = await _service.UpdateProductAsync(input);

        Assert.True(result.Succeeded, result.Message);
        var persistedNotes = type switch
        {
            "Phone" => (await Context.Phones.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            "Apple ID" => (await Context.AppleIds.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            "Tablet" => (await Context.Tablets.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            "Smart Watch" => (await Context.SmartWatches.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            "Laptop" => (await Context.Laptops.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            "Cable" => (await Context.Cables.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            "Charger" => (await Context.Chargers.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            "Power Bank" => (await Context.PowerBanks.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            "Portable Storage" => (await Context.PortableStorages.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            "Case" => (await Context.Cases.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            "Glass" => (await Context.Glasses.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported product type.")
        };
        Assert.Equal("updated note", persistedNotes);

        if (type == "Apple ID")
            Assert.Equal("original plaintext password", (await Context.AppleIds.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Password);
    }

    [Fact]
    public async Task Glass_update_rebuilds_model_name_without_renaming_shared_model()
    {
        var phoneCategory = new Category { Name = "Phone" };
        var glassCategory = new Category { Name = "Glass" };
        var phoneManufacturer = new Manufacturer { Name = $"Phone manufacturer {Guid.NewGuid():N}" };
        var glassManufacturer = new Manufacturer { Name = $"Glass manufacturer {Guid.NewGuid():N}" };
        Context.Categories.AddRange(phoneCategory, glassCategory);
        Context.Manufacturers.AddRange(phoneManufacturer, glassManufacturer);
        await Context.SaveChangesAsync();

        var firstPhone = new Model
        {
            ManufacturerId = phoneManufacturer.Id,
            CategoryId = phoneCategory.Id,
            Name = "iPhone 11"
        };
        var secondPhone = new Model
        {
            ManufacturerId = phoneManufacturer.Id,
            CategoryId = phoneCategory.Id,
            Name = "iPhone 14"
        };
        Context.Models.AddRange(firstPhone, secondPhone);
        await Context.SaveChangesAsync();

        var originalGlassName = MobileShop.Services.DataServices.Shared.GlassModelNaming.Build([firstPhone.Name, secondPhone.Name]);
        var originalGlassModel = new Model
        {
            ManufacturerId = glassManufacturer.Id,
            CategoryId = glassCategory.Id,
            Name = originalGlassName
        };
        Context.Models.Add(originalGlassModel);
        await Context.SaveChangesAsync();

        var product = new Product
        {
            ModelId = originalGlassModel.Id,
            ModelNavigation = originalGlassModel,
            Barcode = Guid.NewGuid().ToString("N")[..12],
            Price = 100,
            GlassProfile = new Glass
            {
                Notes = "Original notes",
                ModelFits =
                [
                    new GlassModelFit { ModelId = firstPhone.Id, ModelNavigation = firstPhone },
                    new GlassModelFit { ModelId = secondPhone.Id, ModelNavigation = secondPhone },
                ]
            }
        };
        var siblingProduct = new Product
        {
            ModelId = originalGlassModel.Id,
            ModelNavigation = originalGlassModel,
            Barcode = Guid.NewGuid().ToString("N")[..12],
            Price = 100,
            GlassProfile = new Glass
            {
                ModelFits =
                [
                    new GlassModelFit { ModelId = firstPhone.Id, ModelNavigation = firstPhone },
                    new GlassModelFit { ModelId = secondPhone.Id, ModelNavigation = secondPhone },
                ]
            }
        };
        Context.Products.AddRange(product, siblingProduct);
        await Context.SaveChangesAsync();

        var unchanged = await _service.UpdateProductAsync(new EditProductInputModel
        {
            ProductId = product.Id,
            Type = "Glass",
            Identifier = product.Barcode,
            Price = product.Price,
            Notes = "Original notes",
            CompatibleModels = [firstPhone.Id, secondPhone.Id]
        });
        Assert.True(unchanged.Succeeded, unchanged.Message);
        Assert.False(unchanged.Changed);
        Assert.Equal(originalGlassModel.Id, product.ModelId);

        var result = await _service.UpdateProductAsync(new EditProductInputModel
        {
            ProductId = product.Id,
            Type = "Glass",
            Identifier = product.Barcode,
            Price = product.Price,
            Notes = "Original notes",
            CompatibleModels = [firstPhone.Id]
        });

        Assert.True(result.Succeeded, result.Message);
        Context.ChangeTracker.Clear();
        var updatedProduct = await Context.Products.AsNoTracking()
            .Include(item => item.ModelNavigation)
            .SingleAsync(item => item.Id == product.Id);
        var updatedGlass = await Context.Glasses.AsNoTracking()
            .Include(item => item.ModelFits)
            .SingleAsync(item => item.ProductId == product.Id);
        var untouchedSibling = await Context.Products.AsNoTracking()
            .Include(item => item.ModelNavigation)
            .SingleAsync(item => item.Id == siblingProduct.Id);

        Assert.Equal(MobileShop.Services.DataServices.Shared.GlassModelNaming.Build([firstPhone.Name]), updatedProduct.ModelNavigation.Name);
        Assert.NotEqual(originalGlassModel.Id, updatedProduct.ModelId);
        Assert.Equal(originalGlassModel.Id, untouchedSibling.ModelId);
        Assert.Equal(originalGlassName, untouchedSibling.ModelNavigation.Name);
        Assert.Equal([firstPhone.Id], updatedGlass.ModelFits.Where(fit => !fit.IsDeleted).Select(fit => fit.ModelId));
    }

    [Fact]
    public async Task UpdateProductAsync_updates_apple_id_security_answers_and_clears_blank_ones()
    {
        var (product, _) = await CreateProductForTypeAsync("Apple ID");
        var input = new EditProductInputModel
        {
            ProductId = product.Id,
            Type = "Apple ID",
            Identifier = product.Barcode,
            Price = product.Price,
            SecurityAnswer1 = "  Sara  ",
            SecurityAnswer2 = "Doctor",
            SecurityAnswer3 = " ",
        };

        var result = await _service.UpdateProductAsync(input);

        Assert.True(result.Succeeded, result.Message);
        var appleId = await Context.AppleIds.AsNoTracking().SingleAsync(item => item.ProductId == product.Id);
        Assert.Equal("Sara", appleId.SecurityAnswer1);
        Assert.Equal("Doctor", appleId.SecurityAnswer2);
        Assert.Null(appleId.SecurityAnswer3);
        Assert.Equal("original plaintext password", appleId.Password);
    }

    [Theory]
    [InlineData("Case")]
    [InlineData("Glass")]
    public async Task UpdateProductAsync_rejects_invalid_compatible_model_sets_without_partial_mutation(string type)
    {
        var (product, compatibleModelId) = await CreateProductForTypeAsync(type);
        var input = new EditProductInputModel
        {
            ProductId = product.Id,
            Type = type,
            Identifier = product.Barcode,
            Price = product.Price,
            Notes = "must not be saved",
            CompatibleModels = [compatibleModelId!.Value, int.MaxValue]
        };

        var result = await _service.UpdateProductAsync(input);

        Assert.False(result.Succeeded);
        Assert.Equal("CompatibleModels", result.ErrorField);
        var persistedNotes = type == "Case"
            ? (await Context.Cases.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes
            : (await Context.Glasses.AsNoTracking().SingleAsync(item => item.ProductId == product.Id)).Notes;
        Assert.Equal("original note", persistedNotes);

        input.CompatibleModels = [compatibleModelId.Value, compatibleModelId.Value];
        var duplicate = await _service.UpdateProductAsync(input);
        Assert.False(duplicate.Succeeded);
        Assert.Equal("CompatibleModels", duplicate.ErrorField);
        var selectedIds = type == "Case"
            ? (await Context.Cases.AsNoTracking().Include(item => item.ModelFits).SingleAsync(item => item.ProductId == product.Id)).ModelFits.Where(fit => !fit.IsDeleted).Select(fit => fit.ModelId).ToArray()
            : (await Context.Glasses.AsNoTracking().Include(item => item.ModelFits).SingleAsync(item => item.ProductId == product.Id)).ModelFits.Where(fit => !fit.IsDeleted).Select(fit => fit.ModelId).ToArray();
        Assert.Equal([compatibleModelId.Value], selectedIds);

        if (type == "Glass")
        {
            input.CompatibleModels = [];
            var empty = await _service.UpdateProductAsync(input);
            Assert.False(empty.Succeeded);
            Assert.Equal(nameof(input.CompatibleModels), empty.ErrorField);
            var stillUnchanged = await Context.Glasses.AsNoTracking()
                .Include(item => item.ModelFits)
                .SingleAsync(item => item.ProductId == product.Id);
            Assert.Equal([compatibleModelId.Value], stillUnchanged.ModelFits.Where(fit => !fit.IsDeleted).Select(fit => fit.ModelId));
        }
    }

    private async Task<(Product Product, int? CompatibleModelId)> CreateProductForTypeAsync(string type)
    {
        var product = TestDataHelpers.CreateProduct(Context);
        int? compatibleModelId = null;

        switch (type)
        {
            case "Phone":
                Context.Phones.Add(new Phone { ProductId = product.Id, IMEI1 = "123456789012345", Notes = "original note" });
                break;
            case "Apple ID":
                Context.AppleIds.Add(new AppleId { ProductId = product.Id, Email = $"{Guid.NewGuid():N}@example.com", Password = "original plaintext password", Notes = "original note", SecurityAnswer1 = "original answer" });
                break;
            case "Tablet":
                Context.Tablets.Add(new Tablet { ProductId = product.Id, Notes = "original note" });
                break;
            case "Smart Watch":
                Context.SmartWatches.Add(new SmartWatch { ProductId = product.Id, Notes = "original note" });
                break;
            case "Laptop":
                var cpu = new Cpu { Name = $"CPU {Guid.NewGuid():N}" };
                Context.Cpus.Add(cpu);
                await Context.SaveChangesAsync();
                Context.Laptops.Add(new Laptop { ProductId = product.Id, CpuId = cpu.Id, CpuNavigation = cpu, DisplaySize = 13.3m, Notes = "original note" });
                break;
            case "Cable":
                Context.Cables.Add(new Cable { ProductId = product.Id, Connector1 = CableConnector.UsbC, Connector2 = CableConnector.UsbA, Length = 1.5m, Notes = "original note" });
                break;
            case "Charger":
                Context.Chargers.Add(new Charger { ProductId = product.Id, Wattage = 20, Pd = true, PortCount = 1, Notes = "original note" });
                break;
            case "Power Bank":
                Context.PowerBanks.Add(new PowerBank
                {
                    ProductId = product.Id, CapacityMah = 10000, MaxWattage = 20, PortCount = 1, Notes = "original note",
                    Ports = [new PowerBankPort { PortNumber = 1, Connector = CableConnector.UsbC }]
                });
                break;
            case "Portable Storage":
                var capacity = new StorageCapacity { Gb = 128 };
                Context.StorageCapacities.Add(capacity);
                await Context.SaveChangesAsync();
                Context.PortableStorages.Add(new PortableStorage
                {
                    ProductId = product.Id, Kind = StorageKind.UsbFlash, StorageCapacityId = capacity.Id,
                    StorageCapacityNavigation = capacity, Speed = 100, Notes = "original note"
                });
                break;
            case "Case":
            case "Glass":
                var phoneCategory = new Category { Name = "Phone" };
                var glassCategory = type == "Glass" ? new Category { Name = "Glass" } : null;
                var phoneManufacturer = new Manufacturer { Name = $"Phone manufacturer {Guid.NewGuid():N}" };
                Context.Categories.Add(phoneCategory);
                if (glassCategory is not null)
                    Context.Categories.Add(glassCategory);
                Context.Manufacturers.Add(phoneManufacturer);
                await Context.SaveChangesAsync();
                var phoneModel = new Model { ManufacturerId = phoneManufacturer.Id, CategoryId = phoneCategory.Id, Name = "Compatible phone model" };
                Context.Models.Add(phoneModel);
                await Context.SaveChangesAsync();
                compatibleModelId = phoneModel.Id;
                if (type == "Case")
                    Context.Cases.Add(new Case { ProductId = product.Id, Notes = "original note", ModelFits = [new CaseModelFit { ModelId = phoneModel.Id, ModelNavigation = phoneModel }] });
                else
                    Context.Glasses.Add(new Glass { ProductId = product.Id, Notes = "original note", ModelFits = [new GlassModelFit { ModelId = phoneModel.Id, ModelNavigation = phoneModel }] });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported product type.");
        }

        await Context.SaveChangesAsync();
        return (product, compatibleModelId);
    }

}
