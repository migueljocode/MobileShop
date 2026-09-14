namespace MobileShop.Web.Pages.Transactions;

public class BuyModel(ITransactionDataService transactionDataService, ISellerDataService sellerDataService) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public IReadOnlyList<Seller> Sellers { get; private set; } = [];
    public string? Message { get; private set; }

    public void OnGet() => LoadSellers();

    public IActionResult OnPost()
    {
        LoadSellers();
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

    private void LoadSellers() => Sellers = sellerDataService.GetAll().ToList();

    public class InputModel
    {
        [Range(1, int.MaxValue)] public int ProductId { get; set; }
        [Range(1, int.MaxValue)] public int SellerId { get; set; }
        [Range(0, double.MaxValue)] public decimal Price { get; set; }
        [DataType(DataType.DateTime)] public DateTime? Date { get; set; }
    }
}
