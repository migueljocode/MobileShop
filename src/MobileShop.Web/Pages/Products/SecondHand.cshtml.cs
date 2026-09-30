namespace MobileShop.Web.Pages.Products;

public class SecondHandModel(
    IProductsDataService dataService) : PageModel
{
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Products = (await dataService.GetSecondHandRowsAsync())
            .OrderBy(product => product.Name)
            .ToList();
    }
}
