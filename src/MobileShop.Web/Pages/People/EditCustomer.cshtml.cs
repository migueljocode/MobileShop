namespace MobileShop.Web.Pages.People;

public class EditCustomerModel(IPeopleDataService dataService) : PageModel
{
    [BindProperty] public CreateCustomerInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    public int BackId { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var details = await dataService.GetCustomerDetailsAsync(id);
        if (details is null)
            return NotFound();

        var parts = details.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        BackId = id;
        Input.FirstName = parts.Length > 0 ? parts[0] : string.Empty;
        Input.LastName = parts.Length > 1 ? parts[1] : string.Empty;
        Input.PhoneNumber = details.PhoneNumber;
        Input.NationalId = details.NationalId;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await dataService.UpdateCustomerAsync(id, Input);
        if (!result.Succeeded)
        {
            Message = result.Message ?? "The customer could not be updated.";
            return Page();
        }

        return RedirectToPage("/People/CustomerDetails", new { id });
    }
}
