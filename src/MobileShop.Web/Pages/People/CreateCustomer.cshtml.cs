namespace MobileShop.Web.Pages.People;

public class CreateCustomerModel(IPeopleDataService dataService) : PageModel
{
    [BindProperty] public CreateCustomerInputModel Input { get; set; } = new();
    public string? Message { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await dataService.CreateCustomerAsync(Input);
        if (!result.Succeeded)
        {
            Message = result.Message ?? "The customer could not be created.";
            return Page();
        }

        return RedirectToPage("/People/Index", new { type = "customers" });
    }

}
