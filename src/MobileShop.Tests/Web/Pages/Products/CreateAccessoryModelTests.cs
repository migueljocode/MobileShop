using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Web.Pages.Products;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Models.Entities;

namespace MobileShop.Tests.Web.Pages.Products;

public class CreateAccessoryModelTests : RepoTestBase
{
    private ProductsDataService Service() => new(
        new BaseRepo<Phone>(Context), new BaseRepo<AppleId>(Context), new BaseRepo<Manufacturer>(Context),
        new BaseRepo<Model>(Context), new BaseRepo<Category>(Context), new BaseRepo<MobileShop.Models.Entities.Color>(Context),
        new BaseRepo<Guarantee>(Context), new BaseRepo<Transaction>(Context), new BaseRepo<PartNumber>(Context),
        new BaseRepo<Product>(Context), new BaseRepo<StorageCapacity>(Context), NullLogger<ProductsDataService>.Instance);

    private (Manufacturer manufacturer, Model model) Seed(string categoryName)
    {
        var manufacturer = new Manufacturer { Name = "Acme " + categoryName };
        var category = new Category { Name = categoryName };
        Context.Manufacturers.Add(manufacturer); Context.Categories.Add(category); Context.SaveChanges();
        var model = new Model { ManufacturerId = manufacturer.Id, CategoryId = category.Id, Name = categoryName + " X" };
        Context.Models.Add(model); Context.SaveChanges();
        return (manufacturer, model);
    }

    [Fact] public async Task Cable_Create_Works()
    {
        var (m, model) = Seed("Cable");
        var page = new CreateCableModel(Service()) { Input = new CreateCableInputModel { ManufacturerId = m.Id, ModelId = model.Id, Connector1 = CableConnector.UsbC, Connector2 = CableConnector.Hdmi, Length = 2, Price = 100, Count = 2 } };
        var result = Assert.IsType<RedirectToPageResult>(await page.OnPostAsync());
        Assert.Equal("cable", result.RouteValues!["type"]); Assert.Equal(2, Context.Products.Count()); Assert.All(Context.Cables, x => Assert.Equal(2m, x.Length));
    }

    [Fact] public async Task Charger_Create_Works()
    {
        var (m, model) = Seed("Charger");
        var page = new CreateChargerModel(Service()) { Input = new CreateChargerInputModel { ManufacturerId = m.Id, ModelId = model.Id, Wattage = 65, Pd = true, PortCount = 2, Price = 100, Count = 2 } };
        Assert.IsType<RedirectToPageResult>(await page.OnPostAsync());
        Assert.Equal(2, Context.Chargers.Count()); Assert.All(Context.Chargers, x => Assert.Equal(65, x.Wattage));
    }

    [Fact] public async Task PowerBank_Create_Works()
    {
        var (m, model) = Seed("PowerBank");
        var page = new CreatePowerBankModel(Service()) { Input = new CreatePowerBankInputModel { ManufacturerId = m.Id, ModelId = model.Id, CapacityMah = 20000, MaxWattage = 30, PortCount = 2, PortTypes = [CableConnector.UsbC, CableConnector.UsbA], Pd = true, Price = 100, Count = 2 } };
        Assert.IsType<RedirectToPageResult>(await page.OnPostAsync());
        Assert.Equal(2, Context.PowerBanks.Count()); Assert.All(Context.PowerBanks, x => Assert.Equal(20000, x.CapacityMah));
        Assert.All(Context.PowerBanks, x => Assert.Equal([CableConnector.UsbC, CableConnector.UsbA], x.Ports.OrderBy(port => port.PortNumber).Select(port => port.Connector).ToArray()));
    }

    [Fact] public async Task PowerBank_Create_Requires_a_type_for_each_port()
    {
        var (m, model) = Seed("PowerBank");
        var page = new CreatePowerBankModel(Service()) { Input = new CreatePowerBankInputModel { ManufacturerId = m.Id, ModelId = model.Id, CapacityMah = 20000, MaxWattage = 30, PortCount = 2, PortTypes = [CableConnector.UsbC], Price = 100 } };

        Assert.IsType<PageResult>(await page.OnPostAsync());
        Assert.Empty(Context.Products);
        Assert.Contains(page.ModelState, entry => entry.Value?.Errors.Count > 0);
    }

    [Fact] public async Task PortableStorage_Create_Works()
    {
        var (m, model) = Seed("PortableStorage");
        var capacity = new StorageCapacity { Gb = 256 }; Context.StorageCapacities.Add(capacity); Context.SaveChanges();
        var page = new CreatePortableStorageModel(Service()) { Input = new CreatePortableStorageInputModel { ManufacturerId = m.Id, ModelId = model.Id, StorageKind = StorageKind.Ssd, StorageCapacityId = capacity.Id, Speed = 1000, Price = 100, Count = 2 } };
        Assert.IsType<RedirectToPageResult>(await page.OnPostAsync());
        Assert.Equal(2, Context.PortableStorages.Count()); Assert.All(Context.PortableStorages, x => Assert.Equal(capacity.Id, x.StorageCapacityId));
    }

    [Fact] public async Task PortableStorage_AddModel_CreatesPortableStorageCategoryModel()
    {
        var manufacturer = new Manufacturer { Name = "Acme Storage" };
        Context.Manufacturers.Add(manufacturer);
        Context.SaveChanges();
        var page = new CreatePortableStorageModel(Service());

        Assert.IsType<JsonResult>(await page.OnPostCreateModelAsync(manufacturer.Id, "Storage X"));

        var category = Context.Categories.Single(item => item.Name == "PortableStorage");
        Assert.Equal(category.Id, Context.Models.Single(item => item.Name == "Storage X").CategoryId);
    }

    [Fact] public async Task PortableStorage_AddCapacity_CreatesSelectableCapacity()
    {
        var page = new CreatePortableStorageModel(Service());

        Assert.IsType<JsonResult>(await page.OnPostCreateStorageCapacityAsync(1024));

        Assert.Equal(1024, Context.StorageCapacities.Single().Gb);
        await page.OnGetAsync();
        Assert.Contains(page.StorageCapacities, capacity => capacity.Name == "1024 GB");
    }

    [Fact] public async Task Case_Create_Works()
    {
        var (caseManufacturer, _) = Seed("Case");
        var phoneManufacturer = new Manufacturer { Name = "PhoneCo" }; var phoneCategory = new Category { Name = "Phone" };
        Context.Manufacturers.Add(phoneManufacturer); Context.Categories.Add(phoneCategory); Context.SaveChanges();
        var phoneModel = new Model { ManufacturerId = phoneManufacturer.Id, CategoryId = phoneCategory.Id, Name = "Phone X" };
        Context.Models.Add(phoneModel); Context.SaveChanges();
        var page = new CreateCaseModel(Service()) { Input = new CreateCaseInputModel { ManufacturerId = caseManufacturer.Id, CompatibleManufacturerId = phoneManufacturer.Id, CompatibleModelIds = [phoneModel.Id], Price = 100, Count = 2 } };
        Assert.IsType<RedirectToPageResult>(await page.OnPostAsync());
        Assert.Equal(2, Context.Cases.Count()); Assert.All(Context.CaseModelFits, x => Assert.Equal(phoneModel.Id, x.ModelId));
    }
}