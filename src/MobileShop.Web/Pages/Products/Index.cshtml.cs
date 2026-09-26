namespace MobileShop.Web.Pages.Products;

public class IndexModel(
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService) : PageModel
{
    public string Type { get; private set; } = "all";
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public async Task OnGetAsync(string? type = null)
    {
        Type = string.IsNullOrWhiteSpace(type) ? "all" : type.ToLowerInvariant();
        var rows = new List<ProductListItemViewModel>();

        if (Type is "all" or "phone")
            rows.AddRange(await phoneDataService.GetInventoryRowsAsync());

        if (Type is "all" or "appleid")
            rows.AddRange(await appleIdDataService.GetInventoryRowsAsync());

        Products = rows;
    }
}
