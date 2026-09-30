namespace MobileShop.Web.Pages.People;

public class CreateSellerModel(IPeopleDataService dataService) : PageModel
{
    [BindProperty] public CreateSellerInputModel Input { get; set; } = new();
    public string? Message { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await dataService.CreateSellerAsync(Input);
        if (!result.Succeeded)
        {
            Message = result.Message ?? "The seller could not be created.";
            return Page();
        }

        return RedirectToPage("/People/Sellers");
    }

}
