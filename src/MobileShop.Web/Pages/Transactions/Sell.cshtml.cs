namespace MobileShop.Web.Pages.Transactions;

public class SellModel(ITransactionDataService transactionDataService, ICustomerDataService customerDataService) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public IReadOnlyList<Customer> Customers { get; private set; } = [];
    public string? Message { get; private set; }

    public void OnGet() => LoadCustomers();

    public IActionResult OnPost()
    {
        LoadCustomers();
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

    private void LoadCustomers() => Customers = customerDataService.GetAll().ToList();

    public class InputModel
    {
        [Range(1, int.MaxValue)] public int ProductId { get; set; }
        [Range(1, int.MaxValue)] public int CustomerId { get; set; }
        [Range(0, double.MaxValue)] public decimal Price { get; set; }
        [DataType(DataType.DateTime)] public DateTime? Date { get; set; }
    }
}
