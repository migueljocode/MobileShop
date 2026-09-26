namespace MobileShop.Web.Pages.People;

public class CustomerDetailsModel(
    ICustomerDataService customerDataService,
    IProductDataService productDataService) : PageModel
{
    public CustomerDetailsViewModel? Customer { get; private set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Customer = await customerDataService.GetDetailsAsync(id);
        if (Customer is null) return NotFound();

        var purchasedProductIds = (await customerDataService.PurchasedProductsAsync(id))
            .Select(product => product.Id)
            .ToHashSet();
        Products = (await productDataService.GetInventoryRowsAsync())
            .Where(row => purchasedProductIds.Contains(row.ProductId))
            .ToList();

        return Page();
    }
}
