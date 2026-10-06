namespace MobileShop.Web.Pages.Products;
public class CreateCableModel(IProductsDataService dataService) : PageModel
{
    [BindProperty] public CreateCableInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    public IReadOnlyList<DropdownOptionViewModel> Manufacturers { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> Models { get; private set; } = [];
    public async Task OnGetAsync()=>await PopulateAsync();
    public async Task<IActionResult> OnGetModelsAsync(int manufacturerId){var o=manufacturerId>0?await dataService.GetModelsAsync(manufacturerId,"Cable"):[];return new JsonResult(o.Select(x=>new{x.Id,x.Name}));}
    public async Task<IActionResult> OnPostAsync(){if(!ModelState.IsValid){await PopulateAsync();return Page();}var r=await dataService.CreateCablesAsync(Input);if(!r.Succeeded){if(r.ErrorField is not null)ModelState.AddModelError(r.ErrorField,r.Message!);else Message=r.Message;await PopulateAsync();return Page();}return RedirectToPage("/Products/Details",new{id=r.EntityId,type="cable"});}
    private async Task PopulateAsync(){Manufacturers=await dataService.GetManufacturersAsync();Models=Input.ManufacturerId>0?await dataService.GetModelsAsync(Input.ManufacturerId,"Cable"):[];}
}