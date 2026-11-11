namespace MobileShop.Web.Pages.People;

public class EditSellerModel(IPeopleDataService dataService) : PageModel
{
    [BindProperty] public CreateSellerInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    public int BackId { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var details = await dataService.GetSellerDetailsAsync(id);
        if (details is null)
            return NotFound();

        BackId = id;
        var parts = details.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Input.FirstName = parts.Length > 0 ? parts[0] : string.Empty;
        Input.LastName = parts.Length > 1 ? parts[1] : string.Empty;
        Input.PhoneNumber = details.PhoneNumber;
        Input.EntityType = Enum.TryParse<SellerEntityType>(details.EntityType, true, out var entityType) ? entityType : SellerEntityType.Real;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await dataService.UpdateSellerAsync(id, Input);
        if (!result.Succeeded)
        {
            Message = result.Message ?? "The seller could not be updated.";
            return Page();
        }

        return RedirectToPage("/People/SellerDetails", new { id });
    }
}
