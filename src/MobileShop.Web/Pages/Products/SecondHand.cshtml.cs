namespace MobileShop.Web.Pages.Products;

public class SecondHandModel(
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService) : PageModel
{
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var rows = (await phoneDataService.GetSecondHandRowsAsync())
            .Concat(await appleIdDataService.GetSecondHandRowsAsync())
            .OrderBy(product => product.Name)
            .ToList();

        Products = rows;
    }
}
