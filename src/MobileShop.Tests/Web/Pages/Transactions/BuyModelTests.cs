using Microsoft.AspNetCore.Mvc;
using Moq;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Services.DataServices.Dal;
using MobileShop.Services.DataServices.Interfaces;
using MobileShop.Web.Pages.Transactions;

namespace MobileShop.Tests.Web.Pages.Transactions;

public class BuyModelTests : RepoTestBase
{
    private readonly BuyModel _model;
    private readonly Mock<ITransactionsDataService> _transactions = new();
    private readonly Mock<IPeopleDataService> _people = new();

    public BuyModelTests()
    {
        var products = new ProductsDataService(
            new BaseRepo<Phone>(Context),
            new BaseRepo<AppleId>(Context),
            new BaseRepo<Manufacturer>(Context),
            new BaseRepo<Model>(Context),
            new BaseRepo<Category>(Context),
            new BaseRepo<Color>(Context),
            new BaseRepo<Guarantee>(Context),
            new BaseRepo<Transaction>(Context),
            new BaseRepo<PartNumber>(Context),
            new BaseRepo<Product>(Context),
            NullLogger<ProductsDataService>.Instance);

        _model = new BuyModel(_transactions.Object, _people.Object, products);
    }

    [Fact]
    public async Task Registry_ContainsDeviceTypes()
    {
        Assert.True(MobileShop.Web.Pages.Shared.ProductCreateRegistry.TryGet("tablet", out var tablet));
        Assert.Equal("_ProductCreateTabletForm", tablet.PartialName);
        Assert.True(MobileShop.Web.Pages.Shared.ProductCreateRegistry.TryGet("smartwatch", out var watch));
        Assert.Equal("CreateSmartWatch", watch.PostHandler);
        Assert.True(MobileShop.Web.Pages.Shared.ProductCreateRegistry.TryGet("laptop", out var laptop));
        Assert.Equal("laptop", laptop.Key);
        Assert.True(MobileShop.Web.Pages.Shared.ProductCreateRegistry.TryGet("cable", out var cable));
        Assert.Equal("CreateCable", cable.PostHandler);
        Assert.True(MobileShop.Web.Pages.Shared.ProductCreateRegistry.TryGet("charger", out var charger));
        Assert.Equal("CreateCharger", charger.PostHandler);
        Assert.True(MobileShop.Web.Pages.Shared.ProductCreateRegistry.TryGet("powerbank", out var powerBank));
        Assert.Equal("CreatePowerBank", powerBank.PostHandler);
        Assert.True(MobileShop.Web.Pages.Shared.ProductCreateRegistry.TryGet("portablestorage", out var storage));
        Assert.Equal("CreatePortableStorage", storage.PostHandler);
        Assert.True(MobileShop.Web.Pages.Shared.ProductCreateRegistry.TryGet("case", out var caseDefinition));
        Assert.Equal("_ProductCreateCaseForm", caseDefinition.PartialName);
    }

    private async Task<(Manufacturer Manufacturer, Model Model)> SeedDeviceAsync(string categoryName)
    {
        var manufacturer = new Manufacturer { Name = "Acme " + categoryName };
        var category = new Category { Name = categoryName };
        Context.Manufacturers.Add(manufacturer);
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();
        var model = new Model { ManufacturerId = manufacturer.Id, CategoryId = category.Id, Name = categoryName + " X" };
        Context.Models.Add(model);
        await Context.SaveChangesAsync();
        return (manufacturer, model);
    }

    [Fact]
    public async Task CreateTabletHandler_CreatesAndReturnsSelectableProduct()
    {
        var (manufacturer, model) = await SeedDeviceAsync("Tablet");
        var created = new ProductListItemViewModel(1, 1, "Tablet", "Acme Tablet X", "Barcode: 123", null, false, false) { SuggestedPrice = 100 };
        _transactions.Setup(service => service.SearchSelectableProductsAsync(TransactionDirection.Buy, null, 500))
            .ReturnsAsync([created]);

        var result = await _model.OnPostCreateTabletAsync(new CreateTabletInputModel { ManufacturerId = manufacturer.Id, ModelId = model.Id, Price = 100 });
        var json = Assert.IsType<JsonResult>(result);
        using var document = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(json.Value));
        Assert.Equal("tablet", document.RootElement.GetProperty("type").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("productId").GetInt32());
        Assert.NotNull(Context.Products.Single().TabletProfile);
    }

    [Fact]
    public async Task CreateSmartWatchHandler_CreatesAndReturnsSelectableProduct()
    {
        var (manufacturer, model) = await SeedDeviceAsync("SmartWatch");
        var created = new ProductListItemViewModel(1, 1, "Smart Watch", "Acme SmartWatch X", "Barcode: 123", null, false, false) { SuggestedPrice = 100 };
        _transactions.Setup(service => service.SearchSelectableProductsAsync(TransactionDirection.Buy, null, 500))
            .ReturnsAsync([created]);

        var result = await _model.OnPostCreateSmartWatchAsync(new CreateSmartWatchInputModel { ManufacturerId = manufacturer.Id, ModelId = model.Id, Price = 100 });
        var json = Assert.IsType<JsonResult>(result);
        using var document = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(json.Value));
        Assert.Equal("smartwatch", document.RootElement.GetProperty("type").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("productId").GetInt32());
        Assert.NotNull(Context.Products.Single().SmartWatchProfile);
    }

    [Fact]
    public async Task CreateLaptopHandler_CreatesAndReturnsSelectableProduct()
    {
        var (manufacturer, model) = await SeedDeviceAsync("Laptop");
        var created = new ProductListItemViewModel(1, 1, "Laptop", "Acme Laptop X", "Barcode: 123", null, false, false) { SuggestedPrice = 100 };
        _transactions.Setup(service => service.SearchSelectableProductsAsync(TransactionDirection.Buy, null, 500))
            .ReturnsAsync([created]);

        var result = await _model.OnPostCreateLaptopAsync(new CreateLaptopInputModel { ManufacturerId = manufacturer.Id, ModelId = model.Id, Price = 100, Cpu = "CPU", Gpu = "GPU", DisplaySize = 15.6m });
        var json = Assert.IsType<JsonResult>(result);
        using var document = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(json.Value));
        Assert.Equal("laptop", document.RootElement.GetProperty("type").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("productId").GetInt32());
        Assert.NotNull(Context.Products.Single().LaptopProfile);
        Assert.Equal("CPU", Context.Products.Single().LaptopProfile!.Cpu);
    }

    [Fact]
    public async Task CreateCableHandler_CreatesAndReturnsSelectableProduct()
    {
        var (manufacturer, model) = await SeedDeviceAsync("Cable");
        _transactions.Setup(service => service.SearchSelectableProductsAsync(TransactionDirection.Buy, null, 500))
            .ReturnsAsync([new ProductListItemViewModel(1, 1, "Cable", "Acme Cable X", "Barcode: 123", null, false, false) { SuggestedPrice = 100 }]);
        var result = await _model.OnPostCreateCableAsync(new CreateCableInputModel { ManufacturerId = manufacturer.Id, ModelId = model.Id, Connector1 = CableConnector.UsbC, Connector2 = CableConnector.Hdmi, Length = 1.5m, Price = 100, Count = 2 });
        var json = Assert.IsType<JsonResult>(result);
        using var document = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(json.Value));
        Assert.Equal("cable", document.RootElement.GetProperty("type").GetString());
        Assert.Equal(2, Context.Products.Count());
        Assert.NotNull(Context.Products.First().CableProfile);
    }

    [Fact]
    public async Task CreateChargerHandler_CreatesAndReturnsSelectableProduct()
    {
        var (manufacturer, model) = await SeedDeviceAsync("Charger");
        _transactions.Setup(service => service.SearchSelectableProductsAsync(TransactionDirection.Buy, null, 500))
            .ReturnsAsync([new ProductListItemViewModel(1, 1, "Charger", "Acme Charger X", "Barcode: 123", null, false, false) { SuggestedPrice = 100 }]);
        var result = await _model.OnPostCreateChargerAsync(new CreateChargerInputModel { ManufacturerId = manufacturer.Id, ModelId = model.Id, Wattage = 65, PortCount = 2, Pd = true, Price = 100, Count = 2 });
        var json = Assert.IsType<JsonResult>(result);
        using var document = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(json.Value));
        Assert.Equal("charger", document.RootElement.GetProperty("type").GetString());
        Assert.Equal(2, Context.Products.Count());
        Assert.True(Context.Products.First().ChargerProfile!.Pd);
    }

    [Fact]
    public async Task CreatePowerBankHandler_CreatesAndReturnsSelectableProduct()
    {
        var (manufacturer, model) = await SeedDeviceAsync("PowerBank");
        _transactions.Setup(service => service.SearchSelectableProductsAsync(TransactionDirection.Buy, null, 500))
            .ReturnsAsync([new ProductListItemViewModel(1, 1, "Power Bank", "Acme PowerBank X", "Barcode: 123", null, false, false) { SuggestedPrice = 100 }]);
        var result = await _model.OnPostCreatePowerBankAsync(new CreatePowerBankInputModel { ManufacturerId = manufacturer.Id, ModelId = model.Id, CapacityMah = 20000, MaxWattage = 30, PortCount = 2, Pd = true, Price = 100, Count = 2 });
        var json = Assert.IsType<JsonResult>(result);
        using var document = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(json.Value));
        Assert.Equal("powerbank", document.RootElement.GetProperty("type").GetString());
        Assert.Equal(2, Context.Products.Count());
        Assert.Equal(20000, Context.Products.First().PowerBankProfile!.CapacityMah);
    }

    [Fact]
    public async Task CreatePortableStorageHandler_CreatesAndReturnsSelectableProduct()
    {
        var (manufacturer, model) = await SeedDeviceAsync("PortableStorage");
        var capacity = new StorageCapacity { Gb = 128 };
        Context.StorageCapacities.Add(capacity);
        await Context.SaveChangesAsync();
        _transactions.Setup(service => service.SearchSelectableProductsAsync(TransactionDirection.Buy, null, 500))
            .ReturnsAsync([new ProductListItemViewModel(1, 1, "Portable Storage", "Acme PortableStorage X", "Barcode: 123", null, false, false) { SuggestedPrice = 100 }]);
        var result = await _model.OnPostCreatePortableStorageAsync(new CreatePortableStorageInputModel { ManufacturerId = manufacturer.Id, ModelId = model.Id, StorageKind = StorageKind.Ssd, StorageCapacityId = capacity.Id, Speed = 1000, Price = 100, Count = 2 });
        var json = Assert.IsType<JsonResult>(result);
        using var document = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(json.Value));
        Assert.Equal("portablestorage", document.RootElement.GetProperty("type").GetString());
        Assert.Equal(2, Context.Products.Count());
        Assert.Equal(128, Context.Products.First().PortableStorageProfile!.StorageCapacityNavigation.Gb);
    }

    [Fact]
    public async Task CreateCaseHandler_CreatesAndReturnsSelectableProduct()
    {
        var caseManufacturer = new Manufacturer { Name = "CaseCo" };
        var phoneManufacturer = new Manufacturer { Name = "PhoneCo" };
        var phoneCategory = new Category { Name = "Phone" };
        var caseCategory = new Category { Name = "Case" };
        Context.Manufacturers.AddRange(caseManufacturer, phoneManufacturer);
        Context.Categories.AddRange(phoneCategory, caseCategory);
        await Context.SaveChangesAsync();
        var phoneModel = new Model { ManufacturerId = phoneManufacturer.Id, CategoryId = phoneCategory.Id, Name = "Phone X" };
        Context.Models.Add(phoneModel);
        await Context.SaveChangesAsync();
        _transactions.Setup(service => service.SearchSelectableProductsAsync(TransactionDirection.Buy, null, 500))
            .ReturnsAsync([new ProductListItemViewModel(1, 1, "Case", "Phone X Case", "Barcode: 123", null, false, false) { SuggestedPrice = 100 }]);
        var result = await _model.OnPostCreateCaseAsync(new CreateCaseInputModel { ManufacturerId = caseManufacturer.Id, CompatibleManufacturerId = phoneManufacturer.Id, CompatibleModelIds = [phoneModel.Id], Price = 100, Count = 2 });
        var json = Assert.IsType<JsonResult>(result);
        using var document = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(json.Value));
        Assert.Equal("case", document.RootElement.GetProperty("type").GetString());
        Assert.Equal(2, Context.Products.Count());
        Assert.NotEmpty(Context.CaseModelFits);
    }

    [Fact]
    public async Task CreatePhoneHandler_CreatesAndReturnsSelectableProduct()
    {
        var manufacturer = new Manufacturer { Name = "Apple" };
        var category = new Category { Name = "Phone" };
        Context.Manufacturers.Add(manufacturer);
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var model = new Model { ManufacturerId = manufacturer.Id, CategoryId = category.Id, Name = "iPhone 16" };
        Context.Models.Add(model);
        await Context.SaveChangesAsync();

        var input = new CreatePhoneInputModel
        {
            ManufacturerId = manufacturer.Id,
            ModelId = model.Id,
            Price = 100,
            IMEI1 = "123456789012345",
        };

        var created = new ProductListItemViewModel(
            1, 1, "Phone", "Apple iPhone 16", "IMEI: 123456789012345", null, false, false)
        {
            SuggestedPrice = 100
        };
        _transactions
            .Setup(service => service.SearchSelectableProductsAsync(TransactionDirection.Buy, null, 500))
            .ReturnsAsync([created]);

        var result = await _model.OnPostCreatePhoneAsync(input);

        var json = Assert.IsType<JsonResult>(result);
        var payload = System.Text.Json.JsonSerializer.Serialize(json.Value);
        using var document = System.Text.Json.JsonDocument.Parse(payload);
        Assert.Equal(1, document.RootElement.GetProperty("productId").GetInt32());
        Assert.Equal("Phone: Apple iPhone 16 — IMEI: 123456789012345", document.RootElement.GetProperty("label").GetString());
        Assert.Single(Context.Phones);
        Assert.Equal(Context.Phones.Single().Id, document.RootElement.GetProperty("productId").GetInt32());
    }
}