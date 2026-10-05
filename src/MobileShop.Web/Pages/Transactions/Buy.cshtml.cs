namespace MobileShop.Web.Pages.Transactions;

public class BuyModel(
    ITransactionsDataService dataService,
    IPeopleDataService peopleDataService) : PageModel
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

    public async Task<IActionResult> OnGetSearchSellersAsync(string? q) =>
        new JsonResult(await peopleDataService.SearchSellersAsync(q));

    public async Task<IActionResult> OnPostCreateSellerAsync(CreateSellerInputModel input)
    {
        ModelState.Clear();
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), validationResults, true))
            return CreateErrorResult("Please correct the seller details.");

        var result = await peopleDataService.CreateSellerAsync(input);
        if (!result.Succeeded || result.EntityId is null)
            return CreateErrorResult(result.Message ?? "The seller could not be created.");

        var name = $"{input.FirstName.Trim()} {input.LastName.Trim()}".Trim();
        return new JsonResult(new DropdownCreateResult(
            true,
            new DropdownOptionViewModel(result.EntityId.Value, name),
            null,
            200));
    }

    private async Task<IActionResult> LoadSelectionsAsync()
    {
        Sellers = await dataService.GetSellersAsync();
        Products = await dataService.GetSelectableProductsAsync(TransactionDirection.Buy);
        return Page();
    }

    private static JsonResult CreateErrorResult(string message) =>
        new(new DropdownCreateResult(false, null, message, 400)) { StatusCode = 400 };
}
