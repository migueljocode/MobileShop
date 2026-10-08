namespace MobileShop.Web.Pages.Transactions;

public class SellModel(
    ITransactionsDataService dataService,
    IPeopleDataService peopleDataService) : PageModel
{
    [BindProperty] public SellInputModel Input { get; set; } = new();
    public IReadOnlyList<PartyOptionViewModel> Customers { get; private set; } = [];
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];
    public string? Message { get; private set; }
    [TempData] public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Input.Date ??= DateTime.Now;
        Message = SuccessMessage;
        return await LoadSelectionsAsync();
    }

    public async Task<IActionResult> OnPostAsync(bool saveAndAddAnother = false)
    {
        await LoadSelectionsAsync();
        if (!ModelState.IsValid) return Page();
        var result = await dataService.RecordSellAsync(Input);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            return Page();
        }
        ModelState.Clear();
        Input = new();
        if (saveAndAddAnother)
        {
            SuccessMessage = "Sale recorded successfully. Ready for the next sale.";
            return RedirectToPage();
        }

        Message = "Sale recorded successfully.";
        return Page();
    }

    public async Task<IActionResult> OnGetSearchProductsAsync(
        string? q,
        string? type = null,
        int? manufacturerId = null,
        int? modelId = null) =>
        new JsonResult(await dataService.SearchSelectableProductsAsync(
            TransactionDirection.Sell, q, type: type, manufacturerId: manufacturerId, modelId: modelId));

    public async Task<IActionResult> OnGetSearchCustomersAsync(string? q) =>
        new JsonResult(await peopleDataService.SearchCustomersAsync(q));

    public async Task<IActionResult> OnPostCreateCustomerAsync(CreateCustomerInputModel input)
    {
        ModelState.Clear();
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), validationResults, true))
            return CreateErrorResult("Please correct the customer details.");

        var result = await peopleDataService.CreateCustomerAsync(input);
        if (!result.Succeeded || result.EntityId is null)
            return CreateErrorResult(result.Message ?? "The customer could not be created.");

        var name = $"{input.FirstName.Trim()} {input.LastName.Trim()}".Trim();
        return new JsonResult(new DropdownCreateResult(
            true,
            new DropdownOptionViewModel(result.EntityId.Value, name),
            null,
            200));
    }

    private async Task<IActionResult> LoadSelectionsAsync()
    {
        Customers = await dataService.GetCustomersAsync();
        Products = await dataService.GetSelectableProductsAsync(TransactionDirection.Sell);
        return Page();
    }

    private static JsonResult CreateErrorResult(string message) =>
        new(new DropdownCreateResult(false, null, message, 400)) { StatusCode = 400 };
}
