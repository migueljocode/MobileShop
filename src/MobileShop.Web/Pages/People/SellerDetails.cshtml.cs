namespace MobileShop.Web.Pages.People;

public class SellerDetailsModel(
    ISellerDataService sellerDataService,
    IProductDataService productDataService) : PageModel
{
    public SellerDetailsViewModel? Seller { get; private set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Seller = await sellerDataService.GetDetailsAsync(id);
        if (Seller is null) return NotFound();

        var suppliedProductIds = (await sellerDataService.SoldToShopAsync(id))
            .Select(product => product.Id)
            .ToHashSet();
        Products = (await productDataService.GetInventoryRowsAsync())
            .Where(row => suppliedProductIds.Contains(row.ProductId))
            .ToList();

        return Page();
    }
}
