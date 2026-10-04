using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Web.Pages.People;

namespace MobileShop.Tests.Web.Pages.People;

/// <summary>Verifies sort-parameter handling on the Customers list page over the real area service.</summary>
public class CustomersModelTests : RepoTestBase
{
    private readonly CustomersModel _model;

    public CustomersModelTests()
    {
        var dataService = new PeopleDataService(
            new BaseRepo<Customer>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Product>(Context),
            NullLogger<PeopleDataService>.Instance);
        _model = new CustomersModel(dataService);
    }

    [Fact]
    public async Task OnGetAsync_DefaultsToNameAscending()
    {
        await _model.OnGetAsync();

        Assert.Equal("Name", _model.SortBy);
        Assert.True(_model.Ascending);
        Assert.Empty(_model.Customers);
    }

    [Fact]
    public async Task OnGetAsync_PassesExplicitSortParamsThrough()
    {
        var person = new Person { FirstName = "Sara", LastName = "Ahmadi", PhoneNumber = "09120000001" };
        Context.People.Add(person);
        Context.SaveChanges();
        Context.Customers.Add(new Customer { PersonId = person.Id, NationalId = "1000000001" });
        Context.SaveChanges();

        await _model.OnGetAsync("Count", ascending: false);

        Assert.Equal("Count", _model.SortBy);
        Assert.False(_model.Ascending);
        Assert.Single(_model.Customers);
        Assert.Equal("Sara Ahmadi", _model.Customers[0].Name);
    }

    [Fact]
    public async Task OnGetAsync_WithInvalidSortBy_KeepsRawValueForHeaderToggleState()
    {
        await _model.OnGetAsync("Bogus");

        Assert.Equal("Bogus", _model.SortBy);
        Assert.True(_model.Ascending);
        Assert.Empty(_model.Customers);
    }
}
