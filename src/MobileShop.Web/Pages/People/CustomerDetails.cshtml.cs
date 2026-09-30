namespace MobileShop.Web.Pages.People;

public class CustomerDetailsModel(IPeopleDataService dataService) : PageModel
{
    public CustomerDetailsViewModel? Customer { get; private set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var details = await dataService.GetCustomerDetailsAsync(id);
        if (details is null) return NotFound();

        Customer = details;
        Products = details.Products;

        return Page();
    }
}
