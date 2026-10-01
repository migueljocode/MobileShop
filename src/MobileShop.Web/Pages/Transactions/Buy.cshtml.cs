namespace MobileShop.Web.Pages.Transactions;

public class BuyModel(
    ITransactionsDataService dataService) : PageModel
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

    private async Task<IActionResult> LoadSelectionsAsync()
    {
        Sellers = await dataService.GetSellersAsync();
        Products = await dataService.GetSelectableProductsAsync(TransactionDirection.Buy);
        return Page();
    }

}
