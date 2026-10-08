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
        var dataService = new ProductsDataService(
            new BaseRepo<Phone>(Context),
            new BaseRepo<AppleId>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Customer>(Context),
            new BaseRepo<Person>(Context),
            new BaseRepo<Manufacturer>(Context),
            new BaseRepo<Model>(Context),
            new BaseRepo<Category>(Context),
            new BaseRepo<Color>(Context),
            new BaseRepo<Guarantee>(Context),
            new BaseRepo<Transaction>(Context),
            new BaseRepo<PartNumber>(Context),
            new BaseRepo<Product>(Context),
            new BaseRepo<StorageCapacity>(Context),
            NullLogger<ProductsDataService>.Instance);

        // seed catalog data that would exist in production seed
        TestDataHelpers.SeedShopSentinels(Context);
        TestDataHelpers.SeedAnisCustomer(Context);
        Context.Categories.Add(new Category { Name = "Phone" });
        Context.Manufacturers.AddRange(
            new Manufacturer { Name = "Apple" },
            new Manufacturer { Name = "Samsung" });
        Context.SaveChanges();

        _model = new CreatePhoneModel(dataService);
    }

    private static CreatePhoneInputModel ValidInput(int manufacturerId, int modelId) => new()
    {
        SellerId = 1,
        ManufacturerId = manufacturerId,
        ModelId = modelId,
        Price = 1500000,
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
    public async Task OnPostAsync_rejects_an_unknown_seller()
    {
        var apple = Context.Manufacturers.First(m => m.Name == "Apple");
        var phoneCategory = Context.Categories.First(c => c.Name == "Phone");
        var model = new Model { ManufacturerId = apple.Id, CategoryId = phoneCategory.Id, Name = "iPhone 16" };
        Context.Models.Add(model);
        Context.SaveChanges();
        _model.Input = ValidInput(apple.Id, model.Id);
        _model.Input.SellerId = 0;

        Assert.IsType<PageResult>(await _model.OnPostAsync());
        Assert.Empty(Context.Phones);
        Assert.Contains(_model.ModelState["SellerId"]!.Errors, error => error.ErrorMessage == "Selected seller not found.");
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

    private (Model model, Model otherModel) SeedModelsWithPartNumbers()
    {
        var apple = Context.Manufacturers.First(m => m.Name == "Apple");
        var phoneCategory = Context.Categories.First(c => c.Name == "Phone");
        var model = new Model { ManufacturerId = apple.Id, CategoryId = phoneCategory.Id, Name = "iPhone 15" };
        var otherModel = new Model { ManufacturerId = apple.Id, CategoryId = phoneCategory.Id, Name = "iPhone 14" };
        Context.Models.AddRange(model, otherModel);
        Context.SaveChanges();

        Context.PartNumbers.AddRange(
            new PartNumber { ModelId = model.Id, Code = "CH/ZAA", SupportsDualSim = true },
            new PartNumber { ModelId = otherModel.Id, Code = "OTHER" });
        Context.SaveChanges();
        return (model, otherModel);
    }

    [Fact]
    public async Task OnGetPartNumbersAsync_ReturnsOnlyOptionsForGivenModel()
    {
        var (model, _) = SeedModelsWithPartNumbers();

        var result = await _model.OnGetPartNumbersAsync(model.Id);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal(1, doc.RootElement.GetArrayLength());
        Assert.Equal("CH/ZAA", doc.RootElement[0].GetProperty("Name").GetString());
    }

    [Fact]
    public async Task OnGetPartNumbersAsync_ReturnsEmptyForInvalidModelId()
    {
        var result = await _model.OnGetPartNumbersAsync(0);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task OnPostCreatePartNumberAsync_CreatesAndReturnsOption()
    {
        var (model, _) = SeedModelsWithPartNumbers();

        var result = await _model.OnPostCreatePartNumberAsync(model.Id, "NEWCODE-1", supportsDualSim: true, supportsEsim: false);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var partNumber = Assert.Single(Context.PartNumbers.Where(pn => pn.Code == "NEWCODE-1" && pn.ModelId == model.Id));
        Assert.True(partNumber.SupportsDualSim);
        Assert.False(partNumber.SupportsEsim);
        var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal("NEWCODE-1", doc.RootElement.GetProperty("name").GetString());
    }


    [Fact]
    public async Task OnPostCreatePartNumberAsync_RejectsMissingModel()
    {
        var result = await _model.OnPostCreatePartNumberAsync(0, "CODE", supportsDualSim: false, supportsEsim: false);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(400, jsonResult.StatusCode);
        Assert.False(Context.PartNumbers.Any(pn => pn.Code == "CODE"));
    }

    [Fact]
    public async Task OnPostCreatePartNumberAsync_DuplicateModelAndCode_ReusesExisting()
    {
        var (model, _) = SeedModelsWithPartNumbers();
        var before = Context.PartNumbers.Count();

        var result = await _model.OnPostCreatePartNumberAsync(model.Id, "CH/ZAA", supportsDualSim: true, supportsEsim: false);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal("CH/ZAA", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal(before, Context.PartNumbers.Count());
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
        Assert.Equal("phone", redirect.RouteValues!["type"]);
        Assert.NotNull(redirect.RouteValues!["id"]);
        Assert.NotNull(Context.Phones.FirstOrDefault(p => p.IMEI1 == "123456789012345"));
    }

    [Fact]
    public async Task OnPostAsync_DuplicateImei_ReturnsModelStateError()
    {
        var apple = Context.Manufacturers.First(m => m.Name == "Apple");
        var phoneCategory = Context.Categories.First(c => c.Name == "Phone");
        var model = new Model { ManufacturerId = apple.Id, CategoryId = phoneCategory.Id, Name = "iPhone 15" };
        Context.Models.Add(model);
        Context.SaveChanges();

        _model.Input = ValidInput(apple.Id, model.Id);
        var first = await _model.OnPostAsync();
        Assert.IsType<RedirectToPageResult>(first);

        _model.Input = ValidInput(apple.Id, model.Id);
        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.True(_model.ModelState.ContainsKey(nameof(CreatePhoneInputModel.IMEI1)));
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

    [Fact]
    public async Task OnGetAsync_LoadsAllColors()
    {
        Context.Colors.AddRange(
            new Color { Name = "Midnight" },
            new Color { Name = "Black" });
        Context.SaveChanges();

        await _model.OnGetAsync();

        Assert.Equal(2, _model.Colors.Count());
        Assert.Contains(_model.Colors, c => c.Name == "Midnight");
        Assert.Contains(_model.Colors, c => c.Name == "Black");
    }

    [Fact]
    public async Task OnPostCreateColorAsync_CreatesNewColor()
    {
        var result = await _model.OnPostCreateColorAsync("Green");

        var jsonResult = Assert.IsType<JsonResult>(result);
        var color = Context.Colors.FirstOrDefault(c => c.Name == "Green");
        Assert.NotNull(color);
    }

    [Fact]
    public async Task OnPostCreateColorAsync_ReturnsExistingIfDuplicate()
    {
        Context.Colors.Add(new Color { Name = "Black" });
        await Context.SaveChangesAsync();
        var blackBefore = Context.Colors.Single(c => c.Name == "Black");

        var result = await _model.OnPostCreateColorAsync("Black");

        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal(blackBefore.Id, doc.RootElement.GetProperty("id").GetInt32());
        Assert.Single(Context.Colors.Where(c => c.Name == "Black"));
    }

    [Fact]
    public async Task OnPostCreateColorAsync_BlankReturns400()
    {
        var result = await _model.OnPostCreateColorAsync("   ");

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(400, jsonResult.StatusCode);
    }

    [Fact]
    public async Task OnPostAsync_WithUnknownColorId_ReturnsValidationError()
    {
        var apple = Context.Manufacturers.First(m => m.Name == "Apple");
        var phoneCategory = Context.Categories.First(c => c.Name == "Phone");
        var model = new Model { ManufacturerId = apple.Id, CategoryId = phoneCategory.Id, Name = "iPhone 15" };
        Context.Models.Add(model);
        Context.SaveChanges();

        _model.Input = ValidInput(apple.Id, model.Id);
        _model.Input.ColorId = 9999; // unknown id

        var result = await _model.OnPostAsync();

        var pageResult = Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.True(_model.ModelState.ContainsKey(nameof(CreatePhoneInputModel.ColorId)));
    }

    [Fact]
    public async Task OnPostAsync_WithValidInput_PersistsColorId()
    {
        var apple = Context.Manufacturers.First(m => m.Name == "Apple");
        var phoneCategory = Context.Categories.First(c => c.Name == "Phone");
        var model = new Model { ManufacturerId = apple.Id, CategoryId = phoneCategory.Id, Name = "iPhone 15" };
        var color = new Color { Name = "Blue" };
        Context.Models.Add(model);
        Context.Colors.Add(color);
        Context.SaveChanges();

        _model.Input = ValidInput(apple.Id, model.Id);
        _model.Input.ColorId = color.Id;

        await _model.OnPostAsync();

        Assert.NotNull(Context.Phones.FirstOrDefault(p => p.ProductNavigation.ColorId == color.Id));
    }

    [Fact]
    public async Task OnPostAsync_ValidationFailure_RepopulatesColors()
    {
        Context.Colors.Add(new Color { Name = "Black" });
        await Context.SaveChangesAsync();

        _model.Input = ValidInput(9999, 1); // invalid manufacturer triggers validation failure

        await _model.OnPostAsync();

        Assert.Single(_model.Colors);
        Assert.Contains(_model.Colors, c => c.Name == "Black");
    }

    [Fact]
    public async Task OnGetAsync_LoadsDistinctCorporations()
    {
        var apple = Context.Manufacturers.First(m => m.Name == "Apple");
        var phoneCategory = Context.Categories.First(c => c.Name == "Phone");
        Context.SaveChanges();
        var products = new[]
        {
            new Product { ModelId = 1, Price = 100 },
            new Product { ModelId = 1, Price = 100 },
            new Product { ModelId = 1, Price = 100 },
        };
        Context.Products.AddRange(products);
        await Context.SaveChangesAsync();
        Context.Guarantees.AddRange(
            new Guarantee { ProductId = products[0].Id, ProductNavigation = products[0], StartDate = DateTime.Today, ExpirationDate = DateTime.Today.AddYears(1), Corporation = "Apple" },
            new Guarantee { ProductId = products[1].Id, ProductNavigation = products[1], StartDate = DateTime.Today, ExpirationDate = DateTime.Today.AddYears(1), Corporation = "apple" },
            new Guarantee { ProductId = products[2].Id, ProductNavigation = products[2], StartDate = DateTime.Today, ExpirationDate = DateTime.Today.AddYears(1), Corporation = "Samsung" });
        await Context.SaveChangesAsync();

        await _model.OnGetAsync();

        Assert.Equal(2, _model.Corporations.Count());
        Assert.Contains(_model.Corporations, c => string.Equals(c, "Apple", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(_model.Corporations, c => c == "Samsung");
    }

    [Fact]
    public async Task OnPostCreateCorporationAsync_NewName_ReturnsNameWithoutDbWrite()
    {
        var before = Context.Guarantees.Count();

        var result = await _model.OnPostCreateCorporationAsync("NewCorp");

        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal("NewCorp", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal(before, Context.Guarantees.Count());
    }

    [Fact]
    public async Task OnPostCreateCorporationAsync_DuplicateCasing_ReturnsExisting()
    {
        var products = new[]
        {
            new Product { ModelId = 1, Price = 100 },
        };
        Context.Products.AddRange(products);
        await Context.SaveChangesAsync();
        Context.Guarantees.Add(new Guarantee { ProductId = products[0].Id, ProductNavigation = products[0], StartDate = DateTime.Today, ExpirationDate = DateTime.Today.AddYears(1), Corporation = "Apple" });
        await Context.SaveChangesAsync();
        await _model.OnGetAsync();

        var result = await _model.OnPostCreateCorporationAsync("apple");

        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal("Apple", doc.RootElement.GetProperty("name").GetString());
        Assert.Single(Context.Guarantees.Where(g => g.Corporation == "Apple"));
    }

    [Fact]
    public async Task OnPostCreateCorporationAsync_BlankReturns400()
    {
        var result = await _model.OnPostCreateCorporationAsync("   ");

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(400, jsonResult.StatusCode);
    }

    [Fact]
    public async Task OnPostAsync_ValidationFailure_RepopulatesCorporations()
    {
        Context.Colors.Add(new Color { Name = "Black" });
        var products = new[]
        {
            new Product { ModelId = 1, Price = 100 },
        };
        Context.Products.AddRange(products);
        await Context.SaveChangesAsync();
        Context.Guarantees.Add(new Guarantee { ProductId = products[0].Id, ProductNavigation = products[0], StartDate = DateTime.Today, ExpirationDate = DateTime.Today.AddYears(1), Corporation = "Apple" });
        await Context.SaveChangesAsync();

        _model.Input = ValidInput(9999, 1); // invalid manufacturer triggers validation failure

        await _model.OnPostAsync();

        Assert.Single(_model.Colors);
        Assert.Single(_model.Corporations);
        Assert.Contains(_model.Corporations, c => c == "Apple");
    }
}
