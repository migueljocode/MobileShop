namespace MobileShop.Web.Pages.People;

public class SellerDetailsModel(ISellerDataService sellerDataService) : PageModel
{
    public Seller? Seller { get; private set; }
    public IReadOnlyList<Product> Products { get; private set; } = [];

    public IActionResult OnGet(int id)
    {
        Seller = sellerDataService.Find(id);
        if (Seller is null) return NotFound();
        Products = sellerDataService.SoldToShop(id).ToList();
        return Page();
    }
}
