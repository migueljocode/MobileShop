using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Web.Pages.People;

namespace MobileShop.Tests.Web.Pages.People;

/// <summary>Verifies sort-parameter handling on the Sellers list page over the real area service.</summary>
public class SellersModelTests : RepoTestBase
{
    private readonly SellersModel _model;

    public SellersModelTests()
    {
        var dataService = new PeopleDataService(
            new BaseRepo<Customer>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Product>(Context),
            NullLogger<PeopleDataService>.Instance);
        _model = new SellersModel(dataService);
    }

    [Fact]
    public async Task OnGetAsync_DefaultsToNameAscending()
    {
        await _model.OnGetAsync();

        Assert.Equal("Name", _model.SortBy);
        Assert.True(_model.Ascending);
        Assert.Empty(_model.Sellers);
    }

    [Fact]
    public async Task OnGetAsync_PassesExplicitSortParamsThrough()
    {
        var person = new Person { FirstName = "Sara", LastName = "Ahmadi", PhoneNumber = "09120000001" };
        Context.People.Add(person);
        Context.SaveChanges();
        Context.Sellers.Add(new Seller { PersonId = person.Id, EntityType = SellerEntityType.Real });
        Context.SaveChanges();

        await _model.OnGetAsync("Phone", ascending: false);

        Assert.Equal("Phone", _model.SortBy);
        Assert.False(_model.Ascending);
        Assert.Single(_model.Sellers);
        Assert.Equal("Sara Ahmadi", _model.Sellers[0].Name);
    }

    [Fact]
    public async Task OnGetAsync_DefaultAscending_WhenQueryOmitsParam()
    {
        // Simulates the ASP.NET binding caveat: omitting the bool query param in a unit call
        // still lands on the method's declared default of true.
        await _model.OnGetAsync("Name");

        Assert.True(_model.Ascending);
        Assert.Empty(_model.Sellers);
    }
}
