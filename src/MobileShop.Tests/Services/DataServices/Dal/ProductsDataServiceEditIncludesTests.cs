namespace MobileShop.Tests.Services.DataServices.Dal;

[Collection("MobileShop-Dal-Serial")]
public class ProductsDataServiceEditIncludesTests : RepoTestBase
{
    private ProductsDataService CreateService() => new(
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

    [Fact]
    public async Task GetProductForEditAsync_loads_case_compatible_model_navigation()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var model = await Context.Models.SingleAsync(m => m.Id == product.ModelId);
        var profile = new Case { ProductId = product.Id };
        Context.Cases.Add(profile);
        await Context.SaveChangesAsync();
        Context.CaseModelFits.Add(new CaseModelFit { CaseId = profile.Id, ModelId = model.Id });
        await Context.SaveChangesAsync();

        var loaded = await CreateService().GetProductForEditAsync(product.Id);

        Assert.NotNull(loaded);
        var fit = Assert.Single(loaded!.CaseProfile!.ModelFits);
        Assert.NotNull(fit.ModelNavigation);
        Assert.Equal(model.Name, fit.ModelNavigation.Name);
    }

    [Fact]
    public async Task GetProductForEditAsync_loads_glass_compatible_model_navigation()
    {
        var product = TestDataHelpers.CreateProduct(Context);
        var model = await Context.Models.SingleAsync(m => m.Id == product.ModelId);
        var profile = new Glass { ProductId = product.Id };
        Context.Glasses.Add(profile);
        await Context.SaveChangesAsync();
        Context.GlassModelFits.Add(new GlassModelFit { GlassId = profile.Id, ModelId = model.Id });
        await Context.SaveChangesAsync();

        var loaded = await CreateService().GetProductForEditAsync(product.Id);

        Assert.NotNull(loaded);
        var fit = Assert.Single(loaded!.GlassProfile!.ModelFits);
        Assert.NotNull(fit.ModelNavigation);
        Assert.Equal(model.Name, fit.ModelNavigation.Name);
    }
}
