namespace MobileShop.Web.Pages.People;

public class SellerDetailsModel(IPeopleDataService dataService) : PageModel
{
    public SellerDetailsViewModel? Seller { get; private set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var details = await dataService.GetSellerDetailsAsync(id);
        if (details is null) return NotFound();

        Seller = details;
        Products = details.Products;

        return Page();
    }
}
