namespace MobileShop.Web.Pages.Products;

public class DetailsModel(
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService,
    ITransactionDataService transactionDataService) : PageModel
{
    public ProductDetailsViewModel? Product { get; private set; }
    public IReadOnlyList<ProductTransactionViewModel> Transactions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, string? type = null)
    {
        if (string.Equals(type, "appleid", StringComparison.OrdinalIgnoreCase))
        {
            Product = await appleIdDataService.GetDetailsAsync(id);
        }
        else
        {
            Product = await phoneDataService.GetDetailsAsync(id);
        }

        if (Product is null) return NotFound();
        Transactions = await transactionDataService.GetProductTransactionsAsync(Product.ProductId);
        return Page();
    }
}
