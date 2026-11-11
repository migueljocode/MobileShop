namespace MobileShop.Web.Pages.People;

public class SellerDetailsModel(IPeopleDataService dataService) : PageModel
{
    public SellerDetailsViewModel? Seller { get; private set; }
    public int SellerId { get; private set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var details = await dataService.GetSellerDetailsAsync(id);
        if (details is null) return NotFound();

        Seller = details;
        SellerId = id;
        Products = details.Products;

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await dataService.SoftDeleteSellerAsync(id);
        if (!result.Succeeded)
        {
            StatusMessage = result.Message ?? "The seller could not be removed.";
            return RedirectToPage(new { id });
        }

        TempData["SuccessMessage"] = "Seller removed.";
        return RedirectToPage("/People/Index", new { type = "sellers" });
    }
}
