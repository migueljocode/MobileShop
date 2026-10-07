using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Web.Pages.People;

namespace MobileShop.Tests.Web.Pages.People;

/// <summary>Minimal coverage for the CustomerDetails page over the real area service.</summary>
public class CustomerDetailsModelTests : RepoTestBase
{
    private readonly CustomerDetailsModel _model;

    public CustomerDetailsModelTests()
    {
        var dataService = new PeopleDataService(
            new BaseRepo<Customer>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Product>(Context),
            NullLogger<PeopleDataService>.Instance);
        _model = new CustomerDetailsModel(dataService);
    }

    [Fact]
    public async Task OnGetAsync_KnownId_AssignsHeaderAndProducts()
    {
        var person = new Person { FirstName = "Ali", LastName = "Rezaei", PhoneNumber = "09120000001" };
        Context.People.Add(person);
        Context.SaveChanges();
        var customer = new Customer { PersonId = person.Id, NationalId = "1000000001" };
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var result = await _model.OnGetAsync(customer.Id);

        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.Customer);
        Assert.Equal("Ali Rezaei", _model.Customer!.Name);
        Assert.Empty(_model.Products);
    }

    [Fact]
    public async Task OnGetAsync_UnknownId_ReturnsNotFound()
    {
        var result = await _model.OnGetAsync(999);

        Assert.IsType<NotFoundResult>(result);
        Assert.Null(_model.Customer);
    }
}

/// <summary>Minimal coverage for the SellerDetails page over the real area service.</summary>
public class SellerDetailsModelTests : RepoTestBase
{
    private readonly SellerDetailsModel _model;

    public SellerDetailsModelTests()
    {
        var dataService = new PeopleDataService(
            new BaseRepo<Customer>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Product>(Context),
            NullLogger<PeopleDataService>.Instance);
        _model = new SellerDetailsModel(dataService);
    }

    [Fact]
    public async Task OnGetAsync_KnownId_AssignsHeaderAndProducts()
    {
        var person = new Person { FirstName = "Sara", LastName = "Karimi", PhoneNumber = "09120000002" };
        Context.People.Add(person);
        Context.SaveChanges();
        var seller = new Seller { PersonId = person.Id, EntityType = SellerEntityType.Legal };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var result = await _model.OnGetAsync(seller.Id);

        Assert.IsType<PageResult>(result);
        Assert.NotNull(_model.Seller);
        Assert.Equal("Sara Karimi", _model.Seller!.Name);
        Assert.Equal("Legal", _model.Seller.EntityType);
        Assert.Empty(_model.Products);
    }

    [Fact]
    public async Task OnGetAsync_UnknownId_ReturnsNotFound()
    {
        var result = await _model.OnGetAsync(999);

        Assert.IsType<NotFoundResult>(result);
        Assert.Null(_model.Seller);
    }
}

/// <summary>Minimal coverage for the CreateCustomer page over the real area service.</summary>
public class CreateCustomerModelTests : RepoTestBase
{
    private readonly CreateCustomerModel _model;

    public CreateCustomerModelTests()
    {
        var dataService = new PeopleDataService(
            new BaseRepo<Customer>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Product>(Context),
            NullLogger<PeopleDataService>.Instance);
        _model = new CreateCustomerModel(dataService);
    }

    [Fact]
    public async Task OnPostAsync_ValidInput_PersistsAndRedirects()
    {
        _model.Input = new CreateCustomerInputModel
        {
            FirstName = "Ali",
            LastName = "Rezaei",
            PhoneNumber = "09120000001",
            NationalId = "1000000001",
        };

        var result = await _model.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/People/Index", redirect.PageName);
        Assert.Equal("customers", redirect.RouteValues!["type"]);
        Assert.Null(_model.Message);
        Assert.Single(Context.Customers);
    }

    [Fact]
    public async Task OnPostAsync_InvalidModelState_ReturnsPageWithoutPersisting()
    {
        _model.ModelState.AddModelError("FirstName", "required");
        _model.Input = new CreateCustomerInputModel();

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Empty(Context.Customers);
    }
}

/// <summary>Minimal coverage for the CreateSeller page over the real area service.</summary>
public class CreateSellerModelTests : RepoTestBase
{
    private readonly CreateSellerModel _model;

    public CreateSellerModelTests()
    {
        var dataService = new PeopleDataService(
            new BaseRepo<Customer>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Product>(Context),
            NullLogger<PeopleDataService>.Instance);
        _model = new CreateSellerModel(dataService);
    }

    [Fact]
    public async Task OnPostAsync_ValidInput_PersistsAndRedirects()
    {
        _model.Input = new CreateSellerInputModel
        {
            FirstName = "Sara",
            LastName = "Karimi",
            PhoneNumber = "09120000002",
            EntityType = SellerEntityType.Real,
        };

        var result = await _model.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/People/Index", redirect.PageName);
        Assert.Equal("sellers", redirect.RouteValues!["type"]);
        Assert.Null(_model.Message);
        Assert.Single(Context.Sellers);
    }

    [Fact]
    public async Task OnPostAsync_InvalidModelState_ReturnsPageWithoutPersisting()
    {
        _model.ModelState.AddModelError("FirstName", "required");
        _model.Input = new CreateSellerInputModel();

        var result = await _model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Empty(Context.Sellers);
    }
}
