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

    public void OnGet() => LoadSelections();

    public IActionResult OnPost()
    {
        LoadSelections();
        if (!ModelState.IsValid) return Page();
        if (!transactionDataService.RecordSell(Input.ProductId, Input.CustomerId, Input.Price, Input.Date))
        {
            ModelState.AddModelError(string.Empty, "The sale could not be recorded. Check the product and price.");
            return Page();
        }
        Message = "Sale recorded successfully.";
        ModelState.Clear();
        Input = new();
        return Page();
    }

    private void LoadSelections()
    {
        Customers = customerDataService.GetPartyOptions();
        Products = phoneDataService.GetSelectableProducts(TransactionDirection.Sell)
            .Concat(appleIdDataService.GetSelectableProducts(TransactionDirection.Sell))
            .OrderBy(product => product.Name)
            .ToList();
    }

}
