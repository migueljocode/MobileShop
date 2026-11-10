namespace MobileShop.Web.Pages.Products;

public class CreateGlassModel(IProductsDataService dataService) : PageModel
{
    [BindProperty] public CreateGlassInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    [TempData] public string? SuccessMessage { get; set; }
    public IReadOnlyList<DropdownOptionViewModel> Manufacturers { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> SelectedFits { get; private set; } = [];
    public DropdownOptionViewModel FitTemplate { get; } = new(0, string.Empty);

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

        SuccessMessage = $"Created {Input.Count} glass product(s) successfully.";
        return RedirectToPage();
    }

    private async Task PopulateDropdownsAsync()
    {
        Manufacturers = await dataService.GetManufacturersAsync();
        SelectedFits = await ResolveSelectedFitsAsync(Input.CompatibleModelIds);
    }

    // Labels every selected phone as "<manufacturer> <model>", in the order it was added.
    private async Task<IReadOnlyList<DropdownOptionViewModel>> ResolveSelectedFitsAsync(IReadOnlyCollection<int> selectedIds)
    {
        if (selectedIds.Count == 0)
            return [];

        var labels = new Dictionary<int, string>();
        foreach (var manufacturer in Manufacturers)
        {
            foreach (var model in await dataService.GetModelsAsync(manufacturer.Id, "Phone"))
                labels[model.Id] = $"{manufacturer.Name} {model.Name}";
        }

        return selectedIds.Distinct()
            .Where(labels.ContainsKey)
            .Select(id => new DropdownOptionViewModel(id, labels[id]))
            .ToList()
            .AsReadOnly();
    }
}
