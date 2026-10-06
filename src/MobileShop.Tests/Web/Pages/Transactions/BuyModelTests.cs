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