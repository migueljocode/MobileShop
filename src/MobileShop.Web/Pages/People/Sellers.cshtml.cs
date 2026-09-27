namespace MobileShop.Web.Pages.People;

public class SellersModel(ISellerDataService sellerDataService) : PageModel
{
    public IReadOnlyList<SellerListItemViewModel> Sellers { get; private set; } = [];

    public string SortBy { get; private set; } = "Name";

    public bool Ascending { get; private set; } = true;

    public async Task OnGetAsync(string sortBy = "Name", bool ascending = true)
    {
        SortBy = sortBy;
        Ascending = ascending;
        Sellers = await sellerDataService.GetListRowsAsync(sortBy, ascending);
    }
}
