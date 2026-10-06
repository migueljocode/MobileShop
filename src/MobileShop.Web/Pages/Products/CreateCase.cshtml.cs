namespace MobileShop.Web.Pages.Products;
public class CreateCaseModel(IProductsDataService dataService) : PageModel
{
    [BindProperty] public CreateCaseInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    public IReadOnlyList<DropdownOptionViewModel> Manufacturers { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> CompatibleModels { get; private set; } = [];
    public async Task OnGetAsync()=>await PopulateAsync();
    public async Task<IActionResult> OnGetCompatibleModelsAsync(int manufacturerId){var o=manufacturerId>0?await dataService.GetModelsAsync(manufacturerId,"Phone"):[];return new JsonResult(o.Select(x=>new{x.Id,x.Name}));}
    public async Task<IActionResult> OnPostAsync(){if(!ModelState.IsValid){await PopulateAsync();return Page();}var r=await dataService.CreateCasesAsync(Input);if(!r.Succeeded){if(r.ErrorField is not null)ModelState.AddModelError(r.ErrorField,r.Message!);else Message=r.Message;await PopulateAsync();return Page();}return RedirectToPage("/Products/Details",new{id=r.EntityId,type="case"});}
    private async Task PopulateAsync(){Manufacturers=await dataService.GetManufacturersAsync();CompatibleModels=Input.CompatibleManufacturerId>0?await dataService.GetModelsAsync(Input.CompatibleManufacturerId,"Phone"):[];}
}