namespace MobileShop.Web.Pages.People;

public class SellerDetailsModel(
    ISellerDataService sellerDataService,
    IProductDataService productDataService) : PageModel
{
    public SellerDetailsViewModel? Seller { get; private set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public IActionResult OnGet(int id)
    {
        Seller = sellerDataService.GetDetails(id);
        if (Seller is null) return NotFound();

        var suppliedProductIds = sellerDataService.SoldToShop(id)
            .Select(product => product.Id)
            .ToHashSet();
        Products = productDataService.GetInventoryRows()
            .Where(row => suppliedProductIds.Contains(row.ProductId))
            .ToList();

        return Page();
    }
}
