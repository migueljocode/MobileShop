using Microsoft.Extensions.Logging.Abstractions;
using MobileShop.Services.DataServices.Dal;
using MobileShop.Dal.Repos;

namespace MobileShop.Tests.Services.DataServices.Dal;

public class ProductDataServiceTests : RepoTestBase
{
    private readonly ProductDataService _service;

    public ProductDataServiceTests()
    {
        _service = new ProductDataService(new ProductRepo(Context), NullLogger<ProductDataService>.Instance);
    }

    [Fact]
    public void GetInventoryRows_returns_all_products_with_projections()
    {
        var product1 = TestDataHelpers.CreateProduct(Context, 100m);
        var product2 = TestDataHelpers.CreateProduct(Context, 200m);
        Context.SaveChanges();

        var rows = _service.GetInventoryRows();

        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.NotNull(r.Name));
        Assert.All(rows, r => Assert.NotNull(r.Type));
    }

    [Fact]
    public void GetInventoryRows_returns_empty_list_when_no_products()
    {
        var rows = _service.GetInventoryRows();
        Assert.Empty(rows);
    }
}
