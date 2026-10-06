namespace MobileShop.Web.Pages.Products;

public class CreateLaptopModel(IProductsDataService dataService) : PageModel
{
    [BindProperty] public CreateLaptopInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    public IReadOnlyList<DropdownOptionViewModel> Manufacturers { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> Models { get; private set; } = [];
    public IReadOnlyList<string> Corporations { get; private set; } = [];
    public async Task OnGetAsync()=>await PopulateDropdownsAsync();
    public async Task<IActionResult> OnGetModelsAsync(int manufacturerId){var o=manufacturerId>0?await dataService.GetModelsAsync(manufacturerId,"Laptop"):[];return new JsonResult(o.Select(x=>new{x.Id,x.Name}));}
    public async Task<IActionResult> OnPostAsync(){if(!ModelState.IsValid){await PopulateDropdownsAsync();return Page();}var result=await dataService.CreateLaptopAsync(Input);if(!result.Succeeded){if(result.ErrorField is not null)ModelState.AddModelError(result.ErrorField,result.Message!);else Message=result.Message;await PopulateDropdownsAsync();return Page();}return RedirectToPage("/Products/Details",new{id=result.EntityId,type="laptop"});}
    private async Task PopulateDropdownsAsync(){Manufacturers=await dataService.GetManufacturersAsync();Models=Input.ManufacturerId>0?await dataService.GetModelsAsync(Input.ManufacturerId,"Laptop"):[];Corporations=await dataService.GetGuaranteeCorporationsAsync();}
}
