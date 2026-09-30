namespace MobileShop.Web.Pages.People;

public class SellersModel(IPeopleDataService dataService) : PageModel
{
    public IReadOnlyList<SellerListItemViewModel> Sellers { get; private set; } = [];

    public string SortBy { get; private set; } = "Name";

    public bool Ascending { get; private set; } = true;

    public async Task OnGetAsync(string sortBy = "Name", bool ascending = true)
    {
        SortBy = sortBy;
        Ascending = ascending;
        Sellers = await dataService.GetSellerRowsAsync(sortBy, ascending);
    }
}
