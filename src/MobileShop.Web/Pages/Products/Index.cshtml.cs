namespace MobileShop.Web.Pages.Products;

public class IndexModel(
    IProductsDataService dataService) : PageModel
{
    public string Type { get; private set; } = "all";
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public async Task OnGetAsync(string? type = null)
    {
        Type = string.IsNullOrWhiteSpace(type) ? "all" : type.ToLowerInvariant();
        Products = await dataService.GetInventoryRowsAsync(Type);
    }
}
