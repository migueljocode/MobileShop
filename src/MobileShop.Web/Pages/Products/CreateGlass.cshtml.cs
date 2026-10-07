namespace MobileShop.Web.Pages.Products;

public class CreateGlassModel(IProductsDataService dataService) : PageModel
{
    [BindProperty] public CreateGlassInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    public IReadOnlyList<DropdownOptionViewModel> Manufacturers { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> CompatibleModels { get; private set; } = [];

    public async Task OnGetAsync() => await PopulateDropdownsAsync();

    public async Task<IActionResult> OnPostCreateManufacturerAsync(string name)
    {
        var result = await dataService.CreateManufacturerAsync(name);
        if (!result.Succeeded)
            return new JsonResult(new { error = result.Error! }) { StatusCode = result.StatusCode };
        return new JsonResult(new { result.Option!.Id, result.Option.Name });
    }

    public async Task<IActionResult> OnPostCreateModelAsync(int manufacturerId, string name)
    {
        var result = await dataService.CreateModelAsync(manufacturerId, name);
        if (!result.Succeeded)
            return new JsonResult(new { error = result.Error! }) { StatusCode = result.StatusCode };
        return new JsonResult(new { result.Option!.Id, result.Option.Name });
    }

    public async Task<IActionResult> OnGetModelsAsync(int manufacturerId)
    {
        var options = manufacturerId > 0 ? await dataService.GetModelsAsync(manufacturerId, "Phone") : [];
        return new JsonResult(options.Select(o => new { o.Id, o.Name }));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return Page();
        }

        var result = await dataService.CreateGlassesAsync(Input);
        if (!result.Succeeded)
        {
            if (result.ErrorField is not null)
                ModelState.AddModelError(result.ErrorField, result.Message!);
            else
                Message = result.Message;

            await PopulateDropdownsAsync();
            return Page();
        }

        Message = $"Created {Input.Count} glass product(s) successfully.";
        await PopulateDropdownsAsync();
        return Page();
    }

    private async Task PopulateDropdownsAsync()
    {
        Manufacturers = await dataService.GetManufacturersAsync();
        CompatibleModels = Input.CompatibleManufacturerId > 0
            ? await dataService.GetModelsAsync(Input.CompatibleManufacturerId, "Phone")
            : [];
    }
}
