namespace MobileShop.Web.Pages.Products;

public class DetailsModel(
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService,
    ITransactionDataService transactionDataService) : PageModel
{
    public ProductDetailsViewModel? Product { get; private set; }
    public IReadOnlyList<ProductTransactionViewModel> Transactions { get; private set; } = [];

    public IActionResult OnGet(int id, string? type = null)
    {
        if (string.Equals(type, "appleid", StringComparison.OrdinalIgnoreCase))
        {
            Product = appleIdDataService.GetDetails(id);
        }
        else
        {
            Product = phoneDataService.GetDetails(id);
        }

        if (Product is null) return NotFound();
        Transactions = transactionDataService.GetProductTransactions(Product.ProductId);
        return Page();
    }
}
