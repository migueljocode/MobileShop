using MobileShop.Web.Pages.Shared;

namespace MobileShop.Web.Pages.Transactions;

public class BuyModel(
    ITransactionsDataService dataService,
    IPeopleDataService peopleDataService,
    IProductsDataService productsDataService) : PageModel
{
    [BindProperty] public BuyInputModel Input { get; set; } = new();
    public IReadOnlyList<PartyOptionViewModel> Sellers { get; private set; } = [];
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];
    public string? Message { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Input.Date ??= DateTime.Today;
        return await LoadSelectionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadSelectionsAsync();
        if (!ModelState.IsValid) return Page();
        var result = await dataService.RecordBuyAsync(Input);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            return Page();
        }
        Message = "Buy recorded successfully.";
        ModelState.Clear();
        Input = new();
        return Page();
    }

    public async Task<IActionResult> OnGetSearchProductsAsync(
        string? q,
        string? type = null,
        int? manufacturerId = null,
        int? modelId = null) =>
        new JsonResult(await dataService.SearchSelectableProductsAsync(
            TransactionDirection.Buy, q, type: type, manufacturerId: manufacturerId, modelId: modelId));

    public async Task<IActionResult> OnGetSearchSellersAsync(string? q) =>
        new JsonResult(await peopleDataService.SearchSellersAsync(q));

    public async Task<IActionResult> OnGetCreateProductFormAsync(string? type)
    {
        if (!MobileShop.Web.Pages.Shared.ProductCreateRegistry.TryGet(type, out var definition))
            return new BadRequestResult();

        switch (definition.Key)
        {
            case "phone":
                ViewData["Manufacturers"] = await productsDataService.GetManufacturersAsync();
                ViewData["Models"] = Array.Empty<DropdownOptionViewModel>();
                ViewData["Colors"] = await productsDataService.GetColorsAsync();
                ViewData["PartNumbers"] = Array.Empty<DropdownOptionViewModel>();
                ViewData["Corporations"] = await productsDataService.GetGuaranteeCorporationsAsync();
                return Partial(definition.PartialName, new CreatePhoneInputModel());

            case "appleid":
                return Partial(definition.PartialName, new CreateAppleIdInputModel());

            case "glass":
                ViewData["Manufacturers"] = await productsDataService.GetManufacturersAsync();
                ViewData["Models"] = Array.Empty<DropdownOptionViewModel>();
                return Partial(definition.PartialName, new CreateGlassInputModel());

            case "tablet":
                return await CreateDeviceFormAsync(definition, "Tablet", new CreateTabletInputModel());
            case "smartwatch":
                return await CreateDeviceFormAsync(definition, "SmartWatch", new CreateSmartWatchInputModel());
            case "laptop":
                return await CreateDeviceFormAsync(definition, "Laptop", new CreateLaptopInputModel());
            case "cable":
                return await CreateAccessoryFormAsync(definition, "Cable", new CreateCableInputModel());
            case "charger":
                return await CreateAccessoryFormAsync(definition, "Charger", new CreateChargerInputModel());
            case "powerbank":
                return await CreateAccessoryFormAsync(definition, "PowerBank", new CreatePowerBankInputModel());
            case "portablestorage":
                return await CreateAccessoryFormAsync(definition, "PortableStorage", new CreatePortableStorageInputModel());
            case "case":
                ViewData["Manufacturers"] = await productsDataService.GetManufacturersAsync();
                ViewData["CompatibleModels"] = Array.Empty<DropdownOptionViewModel>();
                return Partial(definition.PartialName, new CreateCaseInputModel());

            default:
                return new BadRequestResult();
        }
    }

    public async Task<IActionResult> OnGetCreatePhoneModelsAsync(int manufacturerId)
    {
        if (manufacturerId <= 0)
            return new JsonResult(Array.Empty<object>());

        var options = await productsDataService.GetModelsAsync(manufacturerId);
        return new JsonResult(options.Select(option => new { id = option.Id, name = option.Name }));
    }

    public async Task<IActionResult> OnGetCreateGlassModelsAsync(int manufacturerId)
    {
        if (manufacturerId <= 0)
            return new JsonResult(Array.Empty<object>());

        var options = await productsDataService.GetModelsAsync(manufacturerId);
        return new JsonResult(options.Select(option => new { id = option.Id, name = option.Name }));
    }

    public async Task<IActionResult> OnGetCreateAccessoryModelsAsync(int manufacturerId, string? category)
    {
        if (manufacturerId <= 0 || string.IsNullOrWhiteSpace(category))
            return new JsonResult(Array.Empty<object>());
        var allowed = category.Trim();
        if (allowed is not ("Cable" or "Charger" or "PowerBank" or "PortableStorage"))
            return new JsonResult(Array.Empty<object>());
        var options = await productsDataService.GetModelsAsync(manufacturerId, allowed);
        return new JsonResult(options.Select(option => new { id = option.Id, name = option.Name }));
    }

    public async Task<IActionResult> OnGetCreateCaseModelsAsync(int manufacturerId)
    {
        if (manufacturerId <= 0)
            return new JsonResult(Array.Empty<object>());
        var options = await productsDataService.GetModelsAsync(manufacturerId, "Phone");
        return new JsonResult(options.Select(option => new { id = option.Id, name = option.Name }));
    }

    public async Task<IActionResult> OnGetCreateTabletModelsAsync(int manufacturerId) => await GetDeviceModelsAsync(manufacturerId, "Tablet");
    public async Task<IActionResult> OnGetCreateSmartWatchModelsAsync(int manufacturerId) => await GetDeviceModelsAsync(manufacturerId, "SmartWatch");
    public async Task<IActionResult> OnGetCreateLaptopModelsAsync(int manufacturerId) => await GetDeviceModelsAsync(manufacturerId, "Laptop");

    public async Task<IActionResult> OnPostCreatePhoneAsync(CreatePhoneInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input))
            return CreateErrorResult("Please correct the phone details.");

        var result = await productsDataService.CreatePhoneAsync(input);
        return await FinishProductCreateAsync(result, "phone");
    }

    public async Task<IActionResult> OnPostCreateAppleIdAsync(CreateAppleIdInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input))
            return CreateErrorResult("Please correct the Apple ID details.");

        var result = await productsDataService.CreateAppleIdAsync(input);
        return await FinishProductCreateAsync(result, "appleid");
    }

    public async Task<IActionResult> OnPostCreateGlassAsync(CreateGlassInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input))
            return CreateErrorResult("Please correct the glass details.");

        var result = await productsDataService.CreateGlassesAsync(input);
        return await FinishProductCreateAsync(result, "glass");
    }

    public async Task<IActionResult> OnPostCreateTabletAsync(CreateTabletInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input))
            return CreateErrorResult("Please correct the tablet details.");

        var result = await productsDataService.CreateTabletAsync(input);
        return await FinishProductCreateAsync(result, "tablet");
    }

    public async Task<IActionResult> OnPostCreateSmartWatchAsync(CreateSmartWatchInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input))
            return CreateErrorResult("Please correct the smart watch details.");

        var result = await productsDataService.CreateSmartWatchAsync(input);
        return await FinishProductCreateAsync(result, "smartwatch");
    }

    public async Task<IActionResult> OnPostCreateLaptopAsync(CreateLaptopInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input))
            return CreateErrorResult("Please correct the laptop details.");

        var result = await productsDataService.CreateLaptopAsync(input);
        return await FinishProductCreateAsync(result, "laptop");
    }

    public async Task<IActionResult> OnPostCreateCableAsync(CreateCableInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input)) return CreateErrorResult("Please correct the cable details.");
        return await FinishProductCreateAsync(await productsDataService.CreateCablesAsync(input), "cable");
    }

    public async Task<IActionResult> OnPostCreateChargerAsync(CreateChargerInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input)) return CreateErrorResult("Please correct the charger details.");
        return await FinishProductCreateAsync(await productsDataService.CreateChargersAsync(input), "charger");
    }

    public async Task<IActionResult> OnPostCreatePowerBankAsync(CreatePowerBankInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input)) return CreateErrorResult("Please correct the power bank details.");
        return await FinishProductCreateAsync(await productsDataService.CreatePowerBanksAsync(input), "powerbank");
    }

    public async Task<IActionResult> OnPostCreatePortableStorageAsync(CreatePortableStorageInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input)) return CreateErrorResult("Please correct the portable storage details.");
        return await FinishProductCreateAsync(await productsDataService.CreatePortableStoragesAsync(input), "portablestorage");
    }

    public async Task<IActionResult> OnPostCreateCaseAsync(CreateCaseInputModel input)
    {
        ModelState.Clear();
        if (!TryValidate(input)) return CreateErrorResult("Please correct the case details.");
        return await FinishProductCreateAsync(await productsDataService.CreateCasesAsync(input), "case");
    }

    public async Task<IActionResult> OnPostCreateSellerAsync(CreateSellerInputModel input)
    {
        ModelState.Clear();
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), validationResults, true))
            return CreateSellerErrorResult("Please correct the seller details.");

        var result = await peopleDataService.CreateSellerAsync(input);
        if (!result.Succeeded || result.EntityId is null)
            return CreateSellerErrorResult(result.Message ?? "The seller could not be created.");

        var name = $"{input.FirstName.Trim()} {input.LastName.Trim()}".Trim();
        return new JsonResult(new DropdownCreateResult(
            true,
            new DropdownOptionViewModel(result.EntityId.Value, name),
            null,
            200));
    }

    private async Task<IActionResult> FinishProductCreateAsync(ServiceResult result, string type)
    {
        if (!result.Succeeded || result.EntityId is null)
            return CreateErrorResult(result.Message ?? "The product could not be created.");

        var rows = await dataService.SearchSelectableProductsAsync(TransactionDirection.Buy, null, 500);
        var row = rows.FirstOrDefault(item => item.EntityId == result.EntityId.Value);
        if (row is null)
            return CreateErrorResult("The product was created but could not be selected.");

        return new JsonResult(new
        {
            productId = row.ProductId,
            type,
            label = FormatProductLabel(row),
            suggestedPrice = row.SuggestedPrice
        });
    }


    private async Task<IActionResult> CreateDeviceFormAsync<T>(ProductCreateDefinition definition, string category, T input)
    {
        ViewData["Manufacturers"] = await productsDataService.GetManufacturersAsync();
        ViewData["Models"] = Array.Empty<DropdownOptionViewModel>();
        ViewData["Corporations"] = await productsDataService.GetGuaranteeCorporationsAsync();
        ViewData["Category"] = category;
        return Partial(definition.PartialName, input);
    }

    private async Task<IActionResult> CreateAccessoryFormAsync<T>(ProductCreateDefinition definition, string category, T input)
    {
        ViewData["Manufacturers"] = await productsDataService.GetManufacturersAsync();
        ViewData["Models"] = Array.Empty<DropdownOptionViewModel>();
        ViewData["Category"] = category;
        return Partial(definition.PartialName, input);
    }

    private async Task<IActionResult> GetDeviceModelsAsync(int manufacturerId, string category)
    {
        if (manufacturerId <= 0)
            return new JsonResult(Array.Empty<object>());

        var options = await productsDataService.GetModelsAsync(manufacturerId, category);
        return new JsonResult(options.Select(option => new { id = option.Id, name = option.Name }));
    }

    private static bool TryValidate<T>(T input)
    {
        var validationResults = new List<ValidationResult>();
        return Validator.TryValidateObject(input!, new ValidationContext(input!), validationResults, true);
    }

    private async Task<IActionResult> LoadSelectionsAsync()
    {
        Sellers = await dataService.GetSellersAsync();
        Products = await dataService.GetSelectableProductsAsync(TransactionDirection.Buy);
        return Page();
    }

    private static string FormatProductLabel(ProductListItemViewModel row)
    {
        var label = $"{row.Type}: {row.Name} — {row.Identifier}";
        if (!string.IsNullOrWhiteSpace(row.Color))
            label += $" — {row.Color}";
        if (!string.IsNullOrWhiteSpace(row.PartNumberLabel) && row.PartNumberLabel != "N/A")
            label += $" — {row.PartNumberLabel}";
        return label;
    }

    private static JsonResult CreateSellerErrorResult(string message) =>
        new(new DropdownCreateResult(false, null, message, 400)) { StatusCode = 400 };

    private static JsonResult CreateErrorResult(string message) =>
        new(new { error = message }) { StatusCode = 400 };
}