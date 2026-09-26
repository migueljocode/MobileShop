namespace MobileShop.Web.Pages.Transactions;

public class SellModel(
    ITransactionDataService transactionDataService,
    ICustomerDataService customerDataService,
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService) : PageModel
{
    [BindProperty] public SellInputModel Input { get; set; } = new();
    public IReadOnlyList<PartyOptionViewModel> Customers { get; private set; } = [];
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];
    public string? Message { get; private set; }

    public async Task<IActionResult> OnGetAsync()
        => await LoadSelectionsAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadSelectionsAsync();
        if (!ModelState.IsValid) return Page();
        if (!await transactionDataService.RecordSellAsync(Input.ProductId, Input.CustomerId, Input.Price, Input.Date))
        {
            ModelState.AddModelError(string.Empty, "The sale could not be recorded. Check the product and price.");
            return Page();
        }
        Message = "Sale recorded successfully.";
        ModelState.Clear();
        Input = new();
        return Page();
    }

    private async Task<IActionResult> LoadSelectionsAsync()
    {
        Customers = await customerDataService.GetPartyOptionsAsync();
        Products = (await phoneDataService.GetSelectableProductsAsync(TransactionDirection.Sell))
            .Concat(await appleIdDataService.GetSelectableProductsAsync(TransactionDirection.Sell))
            .OrderBy(product => product.Name)
            .ToList();
        return Page();
    }

}
