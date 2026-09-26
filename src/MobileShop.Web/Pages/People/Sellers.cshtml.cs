namespace MobileShop.Web.Pages.People;

public class SellersModel(ISellerDataService sellerDataService) : PageModel
{
    public IReadOnlyList<SellerListItemViewModel> Sellers { get; private set; } = [];

    public async Task OnGetAsync() => Sellers = await sellerDataService.GetListRowsAsync();
}
