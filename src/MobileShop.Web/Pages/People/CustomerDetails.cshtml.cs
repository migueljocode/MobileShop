namespace MobileShop.Web.Pages.People;

public class CustomerDetailsModel(ICustomerDataService customerDataService) : PageModel
{
    public Customer? Customer { get; private set; }
    public IReadOnlyList<Product> Products { get; private set; } = [];

    public IActionResult OnGet(int id)
    {
        Customer = customerDataService.Find(id);
        if (Customer is null) return NotFound();
        Products = customerDataService.PurchasedProducts(id).ToList();
        return Page();
    }
}
