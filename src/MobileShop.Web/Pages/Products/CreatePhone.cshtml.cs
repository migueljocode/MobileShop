namespace MobileShop.Web.Pages.Products;

public class CreatePhoneModel(
    IProductsDataService dataService) : PageModel
{
    [BindProperty] public CreatePhoneInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    public IReadOnlyList<DropdownOptionViewModel> Manufacturers { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> Models { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> Colors { get; private set; } = [];
    public IReadOnlyList<string> Corporations { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> PartNumbers { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await PopulateDropdownsAsync();
    }

    public async Task<IActionResult> OnGetModelsAsync(int manufacturerId)
    {
        var options = await dataService.GetModelsAsync(manufacturerId);
        return new JsonResult(options.Select(o => new { o.Id, o.Name }));
    }

    /// <summary>Returns the model-scoped part numbers so the combobox can reload when the model changes.</summary>
    public async Task<IActionResult> OnGetPartNumbersAsync(int modelId)
    {
        if (modelId <= 0)
            return new JsonResult(Array.Empty<object>());

        var options = await dataService.GetPartNumbersAsync(modelId);
        return new JsonResult(options.Select(o => new { o.Id, o.Name }));
    }

    /// <summary>Creates a part number for the selected model and returns it for immediate selection.</summary>
    public async Task<IActionResult> OnPostCreatePartNumberAsync(
        int modelId,
        string code,
        bool supportsDualSim = false,
        bool supportsEsim = false)
    {
        if (modelId <= 0)
            return new JsonResult(new { error = "Select a model first." }) { StatusCode = 400 };

        var result = await dataService.CreatePartNumberAsync(modelId, code, supportsDualSim, supportsEsim);
        if (!result.Succeeded)
            return new JsonResult(new { error = result.Error! }) { StatusCode = result.StatusCode };

        return new JsonResult(new { id = result.Option!.Id, name = result.Option.Name });
    }

    public async Task<IActionResult> OnPostCreateManufacturerAsync(string name)
    {
        var result = await dataService.CreateManufacturerAsync(name);
        if (!result.Succeeded)
            return new JsonResult(new { error = result.Error! }) { StatusCode = result.StatusCode };

        return new JsonResult(new { id = result.Option!.Id, name = result.Option.Name });
    }

    public async Task<IActionResult> OnPostCreateModelAsync(int manufacturerId, string name)
    {
        var result = await dataService.CreateModelAsync(manufacturerId, name);
        if (!result.Succeeded)
            return new JsonResult(new { error = result.Error! }) { StatusCode = result.StatusCode };

        return new JsonResult(new { id = result.Option!.Id, name = result.Option.Name });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return Page();
        }

        var result = await dataService.CreatePhoneAsync(Input);
        if (!result.Succeeded)
        {
            if (result.ErrorField is not null)
                ModelState.AddModelError(result.ErrorField, result.Message!);
            else
                Message = result.Message;
            await PopulateDropdownsAsync();
            return Page();
        }

        return RedirectToPage("/Products/Details", new { id = result.EntityId, type = "phone" });
    }

    public async Task<IActionResult> OnPostCreateColorAsync(string name)
    {
        var result = await dataService.CreateColorAsync(name);
        if (!result.Succeeded)
            return new JsonResult(new { error = result.Error! }) { StatusCode = result.StatusCode };

        return new JsonResult(new { id = result.Option!.Id, name = result.Option.Name });
    }

    public async Task<IActionResult> OnPostCreateCorporationAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new JsonResult(new { error = "Name is required." }) { StatusCode = 400 };

        var trimmed = name.Trim();
        if (!Corporations.Any())
            await PopulateDropdownsAsync();

        var existing = Corporations.FirstOrDefault(c =>
            string.Equals(c, trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
            return new JsonResult(new { name = existing });

        return new JsonResult(new { name = trimmed });
    }

    private async Task PopulateDropdownsAsync()
    {
        Manufacturers = await dataService.GetManufacturersAsync();
        Models = Input.ManufacturerId > 0
            ? await dataService.GetModelsAsync(Input.ManufacturerId)
            : [];
        Colors = await dataService.GetColorsAsync();
        Corporations = await dataService.GetGuaranteeCorporationsAsync();
        PartNumbers = Input.ModelId > 0
            ? await dataService.GetPartNumbersAsync(Input.ModelId)
            : [];
    }
}
