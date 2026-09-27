using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Web.Pages.Products;

namespace MobileShop.Tests.Web.Pages.Products;

/// <summary>
/// Covers the Create Phone page after the dropdown change: manufacturers and models are
/// selected via cascading dropdowns populated from the repository, with modal POST handlers
/// that create new entities and return JSON for dropdown refresh.
/// </summary>
public class CreatePhoneModelTests : RepoTestBase
{
    private readonly CreatePhoneModel _model;

    public CreatePhoneModelTests()
    {
        var phoneDataService = new PhoneDataService(
            new PhoneRepo(Context),
            NullLogger<PhoneDataService>.Instance);

        // seed catalog data that would exist in production seed
        Context.Categories.Add(new Category { Name = "Phone" });
        Context.Manufacturers.AddRange(
            new Manufacturer { Name = "Apple" },
            new Manufacturer { Name = "Samsung" });
        Context.SaveChanges();

        _model = new CreatePhoneModel(
            phoneDataService,
            new ManufacturerRepo(Context),
            new ModelRepo(Context),
            new CategoryRepo(Context),
            new ColorRepo(Context));
    }

    private static CreatePhoneInputModel ValidInput(int manufacturerId, int modelId) => new()
    {
        ManufacturerId = manufacturerId,
        ModelId = modelId,
        Price = 1_500_000m,
        IMEI1 = "123456789012345",
    };

    [Fact]
    public async Task OnGetAsync_LoadsAllManufacturers()
    {
        await _model.OnGetAsync();

        Assert.Equal(2, _model.Manufacturers.Count());
        Assert.Contains(_model.Manufacturers, m => m.Name == "Apple");
        Assert.Contains(_model.Manufacturers, m => m.Name == "Samsung");
    }

    [Fact]
    public async Task OnGetModelsAsync_ReturnsModelsForManufacturer()
    {
        var apple = Context.Manufacturers.First(m => m.Name == "Apple");
        var phoneCategory = Context.Categories.First(c => c.Name == "Phone");
        Context.Models.AddRange(
            new Model { ManufacturerId = apple.Id, CategoryId = phoneCategory.Id, Name = "iPhone 15" },
            new Model { ManufacturerId = apple.Id, CategoryId = phoneCategory.Id, Name = "iPhone 14" });
        Context.SaveChanges();

        var result = await _model.OnGetModelsAsync(apple.Id);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var models = Assert.IsAssignableFrom<IEnumerable<dynamic>>(jsonResult.Value);
        Assert.Equal(2, models.Count());
    }

    [Fact]
    public async Task OnPostCreateManufacturerAsync_CreatesNewManufacturer()
    {
        var result = await _model.OnPostCreateManufacturerAsync("Google");

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(Context.Manufacturers.FirstOrDefault(m => m.Name == "Google"));
    }

    [Fact]
    public async Task OnPostCreateManufacturerAsync_ReturnsExistingIfDuplicate()
    {
        var appleBefore = Context.Manufacturers.Single(m => m.Name == "Apple");

        var result = await _model.OnPostCreateManufacturerAsync("Apple");

        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal(appleBefore.Id, doc.RootElement.GetProperty("id").GetInt32());
        Assert.Single(Context.Manufacturers.Where(m => m.Name == "Apple"));
    }

    [Fact]
    public async Task OnPostCreateModelAsync_CreatesNewModelWithPhoneCategory()
    {
        var apple = Context.Manufacturers.First(m => m.Name == "Apple");

        var result = await _model.OnPostCreateModelAsync(apple.Id, "iPhone 16");

        var jsonResult = Assert.IsType<JsonResult>(result);
        var model = Context.Models.FirstOrDefault(m => m.Name == "iPhone 16");
        Assert.NotNull(model);
        var phoneCategory = Context.Categories.Single(c => c.Name == "Phone");
        Assert.Equal(phoneCategory.Id, model.CategoryId);
        Assert.Equal(apple.Id, model.ManufacturerId);
    }

    [Fact]
    public async Task OnPostCreateModelAsync_Returns404_WhenManufacturerNotFound()
    {
        var result = await _model.OnPostCreateModelAsync(9999, "Unknown");

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(404, jsonResult.StatusCode);
    }

    [Fact]
    public async Task OnPostAsync_WithValidIds_CreatesPhone()
    {
        var apple = Context.Manufacturers.First(m => m.Name == "Apple");
        var phoneCategory = Context.Categories.First(c => c.Name == "Phone");
        var model = new Model { ManufacturerId = apple.Id, CategoryId = phoneCategory.Id, Name = "iPhone 15" };
        Context.Models.Add(model);
        Context.SaveChanges();

        _model.Input = ValidInput(apple.Id, model.Id);

        var result = await _model.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Products/Details", redirect.PageName);
        Assert.NotNull(Context.Phones.FirstOrDefault(p => p.IMEI1 == "123456789012345"));
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidManufacturerId_ReturnsValidationError()
    {
        var model = Context.Models.FirstOrDefault();
        _model.Input = ValidInput(9999, model?.Id ?? 1);

        var result = await _model.OnPostAsync();

        var pageResult = Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.True(_model.ModelState.ContainsKey(nameof(CreatePhoneInputModel.ManufacturerId)));
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidModelId_ReturnsValidationError()
    {
        var apple = Context.Manufacturers.First(m => m.Name == "Apple");
        _model.Input = ValidInput(apple.Id, 9999);

        var result = await _model.OnPostAsync();

        var pageResult = Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.True(_model.ModelState.ContainsKey(nameof(CreatePhoneInputModel.ModelId)));
    }
}