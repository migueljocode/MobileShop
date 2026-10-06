using Microsoft.AspNetCore.Mvc;
using MobileShop.Models.Entities;
using MobileShop.Models.ViewModels.Web.BindModels;
using MobileShop.Web.Pages.Products;

namespace MobileShop.Tests.Web.Pages.Products;

public class CreateDeviceModelTests : RepoTestBase
{
    private ProductsDataService Service()=>new(new BaseRepo<Phone>(Context),new BaseRepo<AppleId>(Context),new BaseRepo<Manufacturer>(Context),new BaseRepo<Model>(Context),new BaseRepo<Category>(Context),new BaseRepo<MobileShop.Models.Entities.Color>(Context),new BaseRepo<Guarantee>(Context),new BaseRepo<Transaction>(Context),new BaseRepo<PartNumber>(Context),new BaseRepo<Product>(Context),NullLogger<ProductsDataService>.Instance);
    private (Manufacturer,Model) Seed(string c){var m=new Manufacturer{Name="Acme "+c};var cat=new Category{Name=c};Context.Manufacturers.Add(m);Context.Categories.Add(cat);Context.SaveChanges();var model=new Model{ManufacturerId=m.Id,CategoryId=cat.Id,Name=c+" X"};Context.Models.Add(model);Context.SaveChanges();return(m,model);}
    [Fact]public async Task Tablet_Create_Works(){var(x,m)=Seed("Tablet");var p=new CreateTabletModel(Service());await p.OnGetAsync();Assert.Contains(p.Manufacturers,a=>a.Id==x.Id);Assert.Contains("Tablet X",System.Text.Json.JsonSerializer.Serialize((await p.OnGetModelsAsync(x.Id) as JsonResult)!.Value));p.Input=new CreateTabletInputModel{ManufacturerId=x.Id,ModelId=m.Id,Price=100000};var r=Assert.IsType<RedirectToPageResult>(await p.OnPostAsync());Assert.Equal("tablet",r.RouteValues!["type"]);Assert.NotNull(Context.Products.Single(q=>q.Id==(int)r.RouteValues!["id"]).TabletProfile);}
    [Fact]public async Task SmartWatch_Create_Works(){var(x,m)=Seed("SmartWatch");var p=new CreateSmartWatchModel(Service());await p.OnGetAsync();Assert.Contains(p.Manufacturers,a=>a.Id==x.Id);Assert.Contains("SmartWatch X",System.Text.Json.JsonSerializer.Serialize((await p.OnGetModelsAsync(x.Id) as JsonResult)!.Value));p.Input=new CreateSmartWatchInputModel{ManufacturerId=x.Id,ModelId=m.Id,Price=100000};var r=Assert.IsType<RedirectToPageResult>(await p.OnPostAsync());Assert.Equal("smartwatch",r.RouteValues!["type"]);Assert.NotNull(Context.Products.Single(q=>q.Id==(int)r.RouteValues!["id"]).SmartWatchProfile);}
    [Fact]public async Task Laptop_Create_Works(){var(x,m)=Seed("Laptop");var p=new CreateLaptopModel(Service());await p.OnGetAsync();Assert.Contains(p.Manufacturers,a=>a.Id==x.Id);p.Input=new CreateLaptopInputModel{ManufacturerId=x.Id,ModelId=m.Id,Price=100000,Cpu="CPU X",Gpu="GPU X",DisplaySize=15.6m};var r=Assert.IsType<RedirectToPageResult>(await p.OnPostAsync());var l=Context.Products.Single(q=>q.Id==(int)r.RouteValues!["id"]).LaptopProfile;Assert.NotNull(l);Assert.Equal("CPU X",l.Cpu);Assert.Equal("GPU X",l.Gpu);Assert.Equal(15.6m,l.DisplaySize);}
}