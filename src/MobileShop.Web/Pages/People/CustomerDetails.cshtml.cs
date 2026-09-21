namespace MobileShop.Web.Pages.People;

public class CustomerDetailsModel(
    ICustomerDataService customerDataService,
    IProductDataService productDataService) : PageModel
{
    public CustomerDetailsViewModel? Customer { get; private set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public IActionResult OnGet(int id)
    {
        Customer = customerDataService.GetDetails(id);
        if (Customer is null) return NotFound();

        var purchasedProductIds = customerDataService.PurchasedProducts(id)
            .Select(product => product.Id)
            .ToHashSet();
        Products = productDataService.GetInventoryRows()
            .Where(row => purchasedProductIds.Contains(row.ProductId))
            .ToList();

        return Page();
    }
}
