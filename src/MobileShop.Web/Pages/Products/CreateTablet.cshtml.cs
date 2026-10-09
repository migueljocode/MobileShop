namespace MobileShop.Web.Pages.Products;

public class CreateTabletModel(IProductsDataService dataService) : PageModel
{
    [BindProperty] public CreateTabletInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    [TempData] public string? SuccessMessage { get; set; }
    public IReadOnlyList<DropdownOptionViewModel> Manufacturers { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> Models { get; private set; } = [];
    public IReadOnlyList<string> Corporations { get; private set; } = [];
    public async Task OnGetAsync()=>await PopulateDropdownsAsync();
    public async Task<IActionResult> OnGetModelsAsync(int manufacturerId){var o=manufacturerId>0?await dataService.GetModelsAsync(manufacturerId,"Tablet"):[];return new JsonResult(o.Select(x=>new{x.Id,x.Name}));}
    public async Task<IActionResult> OnPostCreateManufacturerAsync(string name)
    {
        var result = await dataService.CreateManufacturerAsync(name);
        if (!result.Succeeded)
            return new JsonResult(new { error = result.Error! }) { StatusCode = result.StatusCode };
        return new JsonResult(new { id = result.Option!.Id, name = result.Option.Name });
    }
    public async Task<IActionResult> OnPostCreateModelAsync(int manufacturerId, string name)
    {
        var result = await dataService.CreateModelAsync(manufacturerId, name, "Tablet");
        if (!result.Succeeded)
            return new JsonResult(new { error = result.Error! }) { StatusCode = result.StatusCode };
        return new JsonResult(new { id = result.Option!.Id, name = result.Option.Name });
    }
    public async Task<IActionResult> OnPostAsync(){if(!ModelState.IsValid){await PopulateDropdownsAsync();return Page();}var result=await dataService.CreateTabletAsync(Input);if(!result.Succeeded){if(result.ErrorField is not null)ModelState.AddModelError(result.ErrorField,result.Message!);else Message=result.Message;await PopulateDropdownsAsync();return Page();}SuccessMessage="Tablet created successfully.";return RedirectToPage();}
    private async Task PopulateDropdownsAsync(){Manufacturers=await dataService.GetManufacturersAsync();Models=Input.ManufacturerId>0?await dataService.GetModelsAsync(Input.ManufacturerId,"Tablet"):[];Corporations=await dataService.GetGuaranteeCorporationsAsync();}
}
