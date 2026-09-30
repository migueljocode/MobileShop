namespace MobileShop.Web.Pages.Products;

public class DetailsModel(
    IProductsDataService dataService) : PageModel
{
    public ProductDetailsViewModel? Product { get; private set; }
    public IReadOnlyList<ProductTransactionViewModel> Transactions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, string? type = null)
    {
        Product = await dataService.GetDetailsAsync(id, type ?? string.Empty);

        if (Product is null) return NotFound();
        Transactions = Product.Transactions;
        return Page();
    }
}
