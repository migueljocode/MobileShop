using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Web.Pages.Products;

namespace MobileShop.Tests.Web.Pages.Products;

[Collection("MobileShop-Dal-Serial")]
public class EditModelTests : RepoTestBase
{
    private readonly ProductsDataService _service;

    public EditModelTests()
    {
        TestDataHelpers.SeedShopSentinels(Context);
        TestDataHelpers.SeedAnisCustomer(Context);
        _service = new ProductsDataService(
            new BaseRepo<Phone>(Context),
            new BaseRepo<AppleId>(Context),
            new BaseRepo<Seller>(Context),
            new BaseRepo<Customer>(Context),
            new BaseRepo<Person>(Context),
            new BaseRepo<Manufacturer>(Context),
            new BaseRepo<Model>(Context),
            new BaseRepo<Category>(Context),
            new BaseRepo<MobileShop.Models.Entities.Color>(Context),
            new BaseRepo<Guarantee>(Context),
            new BaseRepo<Transaction>(Context),
            new BaseRepo<PartNumber>(Context),
            new BaseRepo<Product>(Context),
            new BaseRepo<StorageCapacity>(Context),
            new BaseRepo<Cpu>(Context),
            new BaseRepo<Gpu>(Context),
            NullLogger<ProductsDataService>.Instance);
    }

    [Fact]
    public async Task Cable_post_rejects_missing_connector_and_repopulates_connector_options()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        Context.Cables.Add(new Cable { ProductId = product.Id, Connector1 = CableConnector.UsbC, Connector2 = CableConnector.UsbA, Length = 1.5m });
        await Context.SaveChangesAsync();
        var page = new EditModel(_service)
        {
            Product = new EditProductInputModel
            {
                ProductId = product.Id,
                Type = "Cable",
                Identifier = product.Barcode,
                Price = product.Price,
                Connector1 = null,
                Connector2 = CableConnector.UsbA,
            }
        };

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(page.ModelState.ContainsKey("Product.Connector1"));
        Assert.Contains(page.CableConnectors, option => option.Id == (int)CableConnector.UsbC);
        Assert.Contains(page.CableConnectors, option => option.Id == (int)CableConnector.Lightning);
        var unchanged = await Context.Cables.AsNoTracking().SingleAsync(item => item.ProductId == product.Id);
        Assert.Equal(CableConnector.UsbC, unchanged.Connector1);
    }
}
