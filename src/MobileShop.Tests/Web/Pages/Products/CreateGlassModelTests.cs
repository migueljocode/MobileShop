using MobileShop.Models.ViewModels.Web.BindModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Web.Pages.Products;

namespace MobileShop.Tests.Web.Pages.Products;

public class CreateGlassModelTests : RepoTestBase
{
    private readonly CreateGlassModel _model;

    public CreateGlassModelTests()
    {
        var dataService = new ProductsDataService(
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

        Context.Categories.Add(new Category { Name = "Phone" });
        Context.Manufacturers.AddRange(new Manufacturer { Name = "Apple" }, new Manufacturer { Name = "Samsung" });
        Context.SaveChanges();
        _model = new CreateGlassModel(dataService);
    }

    private (Manufacturer manufacturer, Model model) SeedModel()
    {
        var manufacturer = Context.Manufacturers.First(m => m.Name == "Apple");
        var category = Context.Categories.First(c => c.Name == "Phone");
        var model = new Model { ManufacturerId = manufacturer.Id, CategoryId = category.Id, Name = "iPhone 16" };
        Context.Models.Add(model);
        Context.SaveChanges();
        return (manufacturer, model);
    }

    private static CreateGlassInputModel ValidInput(int compatibleManufacturerId, int compatibleModelId, int glassManufacturerId) => new()
    {
        CompatibleManufacturerId = compatibleManufacturerId,
        CompatibleModelId = compatibleModelId,
        GlassManufacturerId = glassManufacturerId,
        Price = 100m, ProfitPercent = 20m, Count = 3
    };

    [Fact]
    public async Task OnGetAsync_LoadsManufacturers()
    {
        await _model.OnGetAsync();
        Assert.Equal(2, _model.Manufacturers.Count);
        Assert.Contains(_model.Manufacturers, m => m.Name == "Apple");
        Assert.Contains(_model.Manufacturers, m => m.Name == "Samsung");
    }

    [Fact]
    public async Task OnGetModelsAsync_ReturnsModelsForManufacturer()
    {
        var (manufacturer, _) = SeedModel();
        var result = await _model.OnGetModelsAsync(manufacturer.Id);
        var json = Assert.IsType<JsonResult>(result);
        var values = Assert.IsAssignableFrom<IEnumerable<dynamic>>(json.Value);
        Assert.Single(values);
    }

    [Fact]
    public async Task OnPostAsync_InvalidModelState_RedisplaysAndDoesNotCreate()
    {
        var (manufacturer, model) = SeedModel();
        _model.Input = ValidInput(manufacturer.Id, model.Id, Context.Manufacturers.First(m => m.Name == "Samsung").Id);
        _model.ModelState.AddModelError(nameof(CreateGlassInputModel.Count), "Count is invalid.");
        var result = await _model.OnPostAsync();
        Assert.IsType<PageResult>(result);
        Assert.Empty(Context.Products);
        Assert.Equal(2, _model.Manufacturers.Count);
        Assert.Single(_model.CompatibleModels);
    }

    [Fact]
    public async Task OnPostAsync_ValidInput_CreatesRequestedCountAndShowsMessage()
    {
        var (manufacturer, model) = SeedModel();
        _model.Input = ValidInput(manufacturer.Id, model.Id);
        var result = await _model.OnPostAsync();
        Assert.IsType<PageResult>(result);
        Assert.Equal("Created 3 glass product(s) successfully.", _model.Message);
        Assert.Equal(3, Context.Products.Count());
        Assert.All(Context.Glasses, glass => Assert.Equal(model.Id, glass.ModelFits.Single().ModelId));
    }

    [Fact]
    public async Task OnPostAsync_ServiceValidationError_IsAddedToFieldAndRedisplays()
    {
        var (manufacturer, model) = SeedModel();
        _model.Input = ValidInput(manufacturer.Id, model.Id);
        _model.Input.ManufacturerId = 9999;
        var result = await _model.OnPostAsync();
        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.True(_model.ModelState.ContainsKey(nameof(CreateGlassInputModel.ManufacturerId)));
        Assert.Empty(Context.Products);
        Assert.Equal(2, _model.Manufacturers.Count);
    }
}