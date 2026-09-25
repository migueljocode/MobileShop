using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Web.Pages.Products;

namespace MobileShop.Tests.Web.Pages.Products;

/// <summary>
/// Covers the Create Apple ID page after the implicit-model change: the operator supplies only price,
/// email, plaintext password and notes, and every created Apple ID lands on the shared implicit
/// Apple iPhone model inside the seeded AppleId catalog category.
/// </summary>
public class CreateAppleIdModelTests : RepoTestBase
{
    private const string AppleIdCategoryName = "AppleId";
    private const string ImplicitModelName = "iPhone";

    private readonly CreateAppleIdModel _model;

    public CreateAppleIdModelTests()
    {
        var appleIdDataService = new AppleIdDataService(
            new AppleIdRepo(Context),
            NullLogger<AppleIdDataService>.Instance);

        // the AppleId category and the Apple manufacturer come from the catalog seed data in production
        Context.Categories.Add(new Category { Name = AppleIdCategoryName });
        Context.SaveChanges();

        _model = new CreateAppleIdModel(
            appleIdDataService,
            new ManufacturerRepo(Context),
            new ModelRepo(Context),
            new CategoryRepo(Context));
    }

    private static CreateAppleIdInputModel ValidInput(string email) => new()
    {
        Price = 1_500_000m,
        Email = email,
        Password = "plaintext-pass",
        Notes = "created by unit test"
    };

    [Fact]
    public void Input_model_validates_without_a_model_value()
    {
        var input = ValidInput("validates@example.com");

        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);

        Assert.True(valid, string.Join("; ", results.Select(r => r.ErrorMessage)));
    }

    [Fact]
    public void Valid_post_creates_apple_id_on_the_implicit_apple_iphone_model()
    {
        _model.Input = ValidInput("new.appleid@example.com");

        var result = _model.OnPost();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Products/Details", redirect.PageName);

        var appleId = Context.AppleIds.Single(a => a.Email == "new.appleid@example.com");
        Assert.Equal("plaintext-pass", appleId.Password);
        Assert.Equal("created by unit test", appleId.Notes);

        var product = Context.Products.Single(p => p.Id == appleId.ProductId);
        Assert.Equal(1_500_000m, product.Price);

        var model = Context.Models
            .Include(m => m.ManufacturerNavigation)
            .Include(m => m.CategoryNavigation)
            .Single(m => m.Id == product.ModelId);

        Assert.Equal("Apple", model.ManufacturerNavigation.Name);
        Assert.Equal(ImplicitModelName, model.Name);
        Assert.Equal(AppleIdCategoryName, model.CategoryNavigation.Name);
    }

    [Fact]
    public void Duplicate_email_is_rejected_and_creates_nothing()
    {
        _model.Input = ValidInput("duplicate@example.com");
        _model.OnPost();

        var appleIdsBefore = Context.AppleIds.Count();

        _model.Input = ValidInput("duplicate@example.com");
        var result = _model.OnPost();

        Assert.IsType<PageResult>(result);
        Assert.False(_model.ModelState.IsValid);
        Assert.Contains(
            _model.ModelState[nameof(CreateAppleIdInputModel.Email)]!.Errors,
            error => error.ErrorMessage == "This Apple ID email already exists.");
        Assert.Equal(appleIdsBefore, Context.AppleIds.Count());
    }

    [Fact]
    public void Invalid_model_state_redisplays_without_creating()
    {
        _model.Input = ValidInput("invalid@example.com");
        _model.ModelState.AddModelError(nameof(CreateAppleIdInputModel.Email), "The Email field is required.");

        var result = _model.OnPost();

        Assert.IsType<PageResult>(result);
        Assert.Empty(Context.AppleIds.Where(a => a.Email == "invalid@example.com"));
        Assert.Empty(Context.Models.Where(m => m.Name == ImplicitModelName));
    }

    [Fact]
    public void Consecutive_posts_reuse_the_single_implicit_model()
    {
        _model.Input = ValidInput("first@example.com");
        _model.OnPost();

        _model.Input = ValidInput("second@example.com");
        _model.OnPost();

        var implicitModels = Context.Models.Where(m => m.Name == ImplicitModelName).ToList();
        Assert.Single(implicitModels);

        var productIds = Context.AppleIds
            .Where(a => a.Email == "first@example.com" || a.Email == "second@example.com")
            .Select(a => a.ProductId)
            .ToList();
        var modelIds = Context.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => p.ModelId)
            .Distinct()
            .ToList();

        Assert.Single(modelIds);
        Assert.Equal(implicitModels[0].Id, modelIds[0]);
    }
}