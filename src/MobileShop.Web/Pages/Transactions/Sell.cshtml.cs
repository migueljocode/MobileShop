namespace MobileShop.Web.Pages.Transactions;

public class SellModel(
    ITransactionsDataService dataService) : PageModel
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

    private async Task<IActionResult> LoadSelectionsAsync()
    {
        Customers = await dataService.GetCustomersAsync();
        Products = await dataService.GetSelectableProductsAsync(TransactionDirection.Sell);
        return Page();
    }

}
