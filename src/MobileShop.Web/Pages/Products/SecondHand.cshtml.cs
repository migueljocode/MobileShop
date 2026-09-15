namespace MobileShop.Web.Pages.Products;

public class SecondHandModel(
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService) : PageModel
{
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public void OnGet()
    {
        var rows = phoneDataService.GetSecondHandRows()
            .Concat(appleIdDataService.GetSecondHandRows())
            .OrderBy(product => product.Name)
            .ToList();

        Products = rows;
    }
}
