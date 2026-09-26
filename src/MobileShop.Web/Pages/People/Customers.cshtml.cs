namespace MobileShop.Web.Pages.People;

public class CustomersModel(ICustomerDataService customerDataService) : PageModel
{
    public IReadOnlyList<CustomerListItemViewModel> Customers { get; private set; } = [];

    public async Task OnGetAsync() => Customers = await customerDataService.GetListRowsAsync();
}
