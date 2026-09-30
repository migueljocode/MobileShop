namespace MobileShop.Web.Pages.People;

public class CustomersModel(IPeopleDataService dataService) : PageModel
{
    public IReadOnlyList<CustomerListItemViewModel> Customers { get; private set; } = [];

    public string SortBy { get; private set; } = "Name";

    public bool Ascending { get; private set; } = true;

    public async Task OnGetAsync(string sortBy = "Name", bool ascending = true)
    {
        SortBy = sortBy;
        Ascending = ascending;
        Customers = await dataService.GetCustomerRowsAsync(sortBy, ascending);
    }
}
