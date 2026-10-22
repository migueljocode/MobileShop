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
    public async Task Get_edit_model_maps_paid_price_and_profit_from_purchase_transaction()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        product.Price = 7000;
        await Context.SaveChangesAsync();
        Context.Transactions.Add(new Transaction
        {
            ProductId = product.Id,
            SellerId = 1,
            CustomerId = 1,
            FinishedPrice = 5000,
            Date = new DateTime(2026, 1, 1),
            Direction = TransactionDirection.Buy,
        });
        await Context.SaveChangesAsync();
        var page = new EditModel(_service);

        var result = await page.OnGetAsync(product.Id);

        Assert.IsType<PageResult>(result);
        Assert.Equal(5000, page.Product.Price);
        Assert.Equal(2000, page.Product.ProfitAmount);
        Assert.Equal(40m, page.Product.ProfitPercent);
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

    [Fact]
    public async Task Case_edit_populates_phone_model_options_and_uses_model_ids_for_selections()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var phoneModel = await Context.Models.Include(model => model.CategoryNavigation).FirstAsync(model => model.CategoryNavigation.Name == "Phone");
        Context.Cases.Add(new Case { ProductId = product.Id, ModelFits = [new CaseModelFit { ModelId = phoneModel.Id, ModelNavigation = phoneModel }] });
        await Context.SaveChangesAsync();
        var page = new EditModel(_service);
        var result = await page.OnGetAsync(product.Id);
        Assert.IsType<PageResult>(result);
        Assert.Contains(page.CompatibleModelOptions, option => option.Id == phoneModel.Id);
        Assert.Contains(phoneModel.Id, page.Product.CompatibleModels);
    }

}
