namespace MobileShop.Web.Pages.Transactions;

public class SellModel(
    ITransactionsDataService dataService,
    IPeopleDataService peopleDataService) : PageModel
{
    [BindProperty] public SellInputModel Input { get; set; } = new();
    public IReadOnlyList<PartyOptionViewModel> Customers { get; private set; } = [];
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
        var result = await dataService.RecordSellAsync(Input);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message!);
            return Page();
        }
        Message = "Sale recorded successfully.";
        ModelState.Clear();
        Input = new();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateCustomerAsync(CreateCustomerInputModel input)
    {
        if (!ModelState.IsValid)
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
