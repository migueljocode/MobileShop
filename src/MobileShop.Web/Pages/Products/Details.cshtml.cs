namespace MobileShop.Web.Pages.Products;

public class DetailsModel(
    IProductsDataService dataService) : PageModel
{
    public ProductDetailsViewModel? Product { get; private set; }
    public IReadOnlyList<ProductTransactionViewModel> Transactions { get; private set; } = [];

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, string? type = null)
    {
        Product = await dataService.GetDetailsAsync(id, type ?? string.Empty);

        if (Product is null) return NotFound();
        Transactions = Product.Transactions;
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, string? type = null)
    {
        var result = await dataService.SoftDeleteProductAsync(id);
        if (!result.Succeeded)
        {
            SuccessMessage = result.Message ?? "The product could not be removed.";
            return RedirectToPage(new { id, type });
        }

        SuccessMessage = "Product removed.";
        var indexType = type switch
        {
            "Apple ID" => "appleid",
            "Smart Watch" => "smartwatch",
            "Power Bank" => "powerbank",
            "Portable Storage" => "portablestorage",
            _ => type?.ToLowerInvariant() ?? "all"
        };
        return RedirectToPage("/Products/Index", new { type = indexType });
    }
}
