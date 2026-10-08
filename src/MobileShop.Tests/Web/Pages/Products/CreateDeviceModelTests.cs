using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MobileShop.Models.Entities;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Web.Pages.Products;

namespace MobileShop.Tests.Web.Pages.Products;

public class CreateDeviceModelTests : RepoTestBase
{
    public CreateDeviceModelTests(){TestDataHelpers.SeedShopSentinels(Context);TestDataHelpers.SeedAnisCustomer(Context);}
    private ProductsDataService Service()=>new(new BaseRepo<Phone>(Context),new BaseRepo<AppleId>(Context),new BaseRepo<Seller>(Context),new BaseRepo<Customer>(Context),new BaseRepo<Person>(Context),new BaseRepo<Manufacturer>(Context),new BaseRepo<Model>(Context),new BaseRepo<Category>(Context),new BaseRepo<MobileShop.Models.Entities.Color>(Context),new BaseRepo<Guarantee>(Context),new BaseRepo<Transaction>(Context),new BaseRepo<PartNumber>(Context),new BaseRepo<Product>(Context),new BaseRepo<StorageCapacity>(Context),NullLogger<ProductsDataService>.Instance);
    private (Manufacturer,Model) Seed(string c){var m=new Manufacturer{Name="Acme "+c};var cat=new Category{Name=c};Context.Manufacturers.Add(m);Context.Categories.Add(cat);Context.SaveChanges();var model=new Model{ManufacturerId=m.Id,CategoryId=cat.Id,Name=c+" X"};Context.Models.Add(model);Context.SaveChanges();return(m,model);}
    [Fact]public async Task Tablet_Create_Works(){var(x,m)=Seed("Tablet");var p=new CreateTabletModel(Service());await p.OnGetAsync();Assert.Contains(p.Manufacturers,a=>a.Id==x.Id);Assert.Contains("Tablet X",System.Text.Json.JsonSerializer.Serialize((await p.OnGetModelsAsync(x.Id) as JsonResult)!.Value));p.Input=new CreateTabletInputModel{SellerId=1,ManufacturerId=x.Id,ModelId=m.Id,Price=100000};var r=Assert.IsType<RedirectToPageResult>(await p.OnPostAsync());Assert.Equal("tablet",r.RouteValues!["type"]);Assert.NotNull(Context.Products.Single(q=>q.Id==Assert.IsType<int>(r.RouteValues!["id"])).TabletProfile);}
    [Fact]public async Task Tablet_AddModelThenCreate_Works()
    {
        var (manufacturer, _) = Seed("Phone");
        var page = new CreateTabletModel(Service());

        Assert.IsType<JsonResult>(await page.OnPostCreateModelAsync(manufacturer.Id, "New Tablet"));

        var model = Context.Models.Single(item => item.Name == "New Tablet");
        Assert.Equal("Tablet", Context.Categories.Single(category => category.Id == model.CategoryId).Name);
        page.Input = new CreateTabletInputModel { SellerId = 1, ManufacturerId = manufacturer.Id, ModelId = model.Id, Price = 100000 };
        var result = Assert.IsType<RedirectToPageResult>(await page.OnPostAsync());
        Assert.NotNull(Context.Products.Single(product => product.Id == Assert.IsType<int>(result.RouteValues!["id"])).TabletProfile);
    }
    [Fact]public async Task SmartWatch_AddModelThenCreate_Works()
    {
        var (manufacturer, _) = Seed("SmartWatch");
        var page = new CreateSmartWatchModel(Service());
        await page.OnGetAsync();

        var createModelResult = Assert.IsType<JsonResult>(await page.OnPostCreateModelAsync(manufacturer.Id, "New SmartWatch"));
        Assert.NotNull(createModelResult.Value);
        var model = Context.Models.Single(item => item.Name == "New SmartWatch");
        Assert.Equal("SmartWatch", Context.Categories.Single(category => category.Id == model.CategoryId).Name);

        page.Input = new CreateSmartWatchInputModel { SellerId = 1, ManufacturerId = manufacturer.Id, ModelId = model.Id, Price = 100000 };
        var result = Assert.IsType<RedirectToPageResult>(await page.OnPostAsync());
        Assert.Equal("smartwatch", result.RouteValues!["type"]);
        Assert.NotNull(Context.Products.Single(product => product.Id == Assert.IsType<int>(result.RouteValues!["id"])).SmartWatchProfile);
    }
    [Fact]public async Task Laptop_AddModelThenCreate_Works(){var(x,_)=Seed("Laptop");var p=new CreateLaptopModel(Service());await p.OnGetAsync();Assert.Contains(p.Manufacturers,a=>a.Id==x.Id);Assert.IsType<JsonResult>(await p.OnPostCreateModelAsync(x.Id,"Laptop X"));var m=Context.Models.Single(q=>q.Name=="Laptop X");Assert.Equal("Laptop",Context.Categories.Single(q=>q.Id==m.CategoryId).Name);p.Input=new CreateLaptopInputModel{SellerId=1,ManufacturerId=x.Id,ModelId=m.Id,Price=100000,Cpu="CPU X",Gpu="GPU X",DisplaySize=15.6m};var r=Assert.IsType<RedirectToPageResult>(await p.OnPostAsync());var l=Context.Products.Single(q=>q.Id==Assert.IsType<int>(r.RouteValues!["id"])).LaptopProfile;Assert.NotNull(l);Assert.Equal("CPU X",l.Cpu);Assert.Equal("GPU X",l.Gpu);Assert.Equal(15.6m,l.DisplaySize);}
    [Fact]public async Task Laptop_Create_RejectsZeroDisplaySize(){var(x,m)=Seed("Laptop");var p=new CreateLaptopModel(Service()){Input=new CreateLaptopInputModel{ManufacturerId=x.Id,ModelId=m.Id,Price=100000,Cpu="CPU X",Gpu="GPU X",DisplaySize=0}};Assert.IsType<PageResult>(await p.OnPostAsync());Assert.Contains("greater than 0",p.Message);Assert.Empty(Context.Products);}
    [Fact]public async Task Laptop_Create_AllowsMissingGpu(){var(x,m)=Seed("Laptop");var p=new CreateLaptopModel(Service()){Input=new CreateLaptopInputModel{SellerId=1,ManufacturerId=x.Id,ModelId=m.Id,Price=100000,Cpu="CPU X",DisplaySize=15.6m}};p.ModelState.AddModelError("Input.Gpu","The GPU field is required.");var result=Assert.IsType<RedirectToPageResult>(await p.OnPostAsync());var laptop=Context.Products.Single(product=>product.Id==Assert.IsType<int>(result.RouteValues!["id"])).LaptopProfile;Assert.NotNull(laptop);Assert.Equal(string.Empty,laptop.Gpu);}
}