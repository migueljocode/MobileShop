namespace MobileShop.Web.Pages.People;

public class CustomerDetailsModel(IPeopleDataService dataService) : PageModel
{
    public CustomerDetailsViewModel? Customer { get; private set; }
    public int CustomerId { get; private set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var details = await dataService.GetCustomerDetailsAsync(id);
        if (details is null) return NotFound();

        Customer = details;
        CustomerId = id;
        Products = details.Products;

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await dataService.SoftDeleteCustomerAsync(id);
        if (!result.Succeeded)
        {
            StatusMessage = result.Message ?? "The customer could not be removed.";
            return RedirectToPage(new { id });
        }

        TempData["SuccessMessage"] = "Customer removed.";
        return RedirectToPage("/People/Index", new { type = "customers" });
    }
}
