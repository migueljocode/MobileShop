namespace MobileShop.Web.Pages.Products;
public class CreateCableModel(IProductsDataService dataService) : PageModel
{
    [BindProperty] public CreateCableInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    [TempData] public string? SuccessMessage { get; set; }
    public IReadOnlyList<DropdownOptionViewModel> Manufacturers { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> Models { get; private set; } = [];
    public async Task OnGetAsync()=>await PopulateAsync();
    public async Task<IActionResult> OnGetModelsAsync(int manufacturerId){var o=manufacturerId>0?await dataService.GetModelsAsync(manufacturerId,"Cable"):[];return new JsonResult(o.Select(x=>new{x.Id,x.Name}));}
    public async Task<IActionResult> OnPostCreateManufacturerAsync(string name){var result=await dataService.CreateManufacturerAsync(name);if(!result.Succeeded)return new JsonResult(new { error = result.Error! }) { StatusCode = result.StatusCode };return new JsonResult(new { id = result.Option!.Id, name = result.Option.Name });}
    public async Task<IActionResult> OnPostCreateModelAsync(int manufacturerId, string name){var result=await dataService.CreateModelAsync(manufacturerId, name, "Cable");if(!result.Succeeded)return new JsonResult(new { error = result.Error! }) { StatusCode = result.StatusCode };return new JsonResult(new { id = result.Option!.Id, name = result.Option.Name });}
    public async Task<IActionResult> OnPostAsync(){if(!ModelState.IsValid){await PopulateAsync();return Page();}var r=await dataService.CreateCablesAsync(Input);if(!r.Succeeded){if(r.ErrorField is not null)ModelState.AddModelError(r.ErrorField,r.Message!);else Message=r.Message;await PopulateAsync();return Page();}SuccessMessage="Cable created successfully.";return RedirectToPage();}
    private async Task PopulateAsync(){Manufacturers=await dataService.GetManufacturersAsync();Models=Input.ManufacturerId>0?await dataService.GetModelsAsync(Input.ManufacturerId,"Cable"):[];}
}