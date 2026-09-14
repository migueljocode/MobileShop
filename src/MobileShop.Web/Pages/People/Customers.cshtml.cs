namespace MobileShop.Web.Pages.People;

public class CustomersModel(ICustomerDataService customerDataService) : PageModel
{
    public IReadOnlyList<Customer> Customers { get; private set; } = [];
    public void OnGet() => Customers = customerDataService.GetAll().ToList();
}
