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