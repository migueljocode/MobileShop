namespace MobileShop.Web.Pages.Transactions;

public class BuyModel(
    ITransactionDataService transactionDataService,
    ISellerDataService sellerDataService,
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService) : PageModel
{
    [BindProperty] public BuyInputModel Input { get; set; } = new();
    public IReadOnlyList<PartyOptionViewModel> Sellers { get; private set; } = [];
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];
    public string? Message { get; private set; }

    public async Task<IActionResult> OnGetAsync()
        => await LoadSelectionsAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadSelectionsAsync();
        if (!ModelState.IsValid) return Page();
        if (!await transactionDataService.RecordBuyAsync(Input.ProductId, Input.SellerId, Input.Price, Input.Date))
        {
            ModelState.AddModelError(string.Empty, "The buy could not be recorded. Check the product and price.");
            return Page();
        }
        Message = "Buy recorded successfully.";
        ModelState.Clear();
        Input = new();
        return Page();
    }

    private async Task<IActionResult> LoadSelectionsAsync()
    {
        Sellers = await sellerDataService.GetPartyOptionsAsync();
        Products = (await phoneDataService.GetSelectableProductsAsync(TransactionDirection.Buy))
            .Concat(await appleIdDataService.GetSelectableProductsAsync(TransactionDirection.Buy))
            .OrderBy(product => product.Name)
            .ToList();
        return Page();
    }

}
