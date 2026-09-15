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

    public void OnGet() => LoadSelections();

    public IActionResult OnPost()
    {
        LoadSelections();
        if (!ModelState.IsValid) return Page();
        if (!transactionDataService.RecordBuy(Input.ProductId, Input.SellerId, Input.Price, Input.Date))
        {
            ModelState.AddModelError(string.Empty, "The buy could not be recorded. Check the product and price.");
            return Page();
        }
        Message = "Buy recorded successfully.";
        ModelState.Clear();
        Input = new();
        return Page();
    }

    private void LoadSelections()
    {
        Sellers = sellerDataService.GetPartyOptions();
        Products = phoneDataService.GetSelectableProducts(TransactionDirection.Buy)
            .Concat(appleIdDataService.GetSelectableProducts(TransactionDirection.Buy))
            .OrderBy(product => product.Name)
            .ToList();
    }

}
