namespace MobileShop.Web.Pages.People;

public class SellersModel(ISellerDataService sellerDataService) : PageModel
{
    public IReadOnlyList<Seller> Sellers { get; private set; } = [];
    public void OnGet() => Sellers = sellerDataService.GetAll().ToList();
}
