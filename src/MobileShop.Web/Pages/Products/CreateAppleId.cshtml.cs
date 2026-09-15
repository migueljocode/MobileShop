namespace MobileShop.Web.Pages.Products;

public class CreateAppleIdModel(IAppleIdDataService appleIdDataService) : PageModel
{
    [BindProperty] public CreateAppleIdInputModel Input { get; set; } = new();
    public string? Message { get; private set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim();
        if (appleIdDataService.FindByEmail(email) is not null)
        {
            ModelState.AddModelError(nameof(Input.Email), "This Apple ID email already exists.");
            return Page();
        }

        var product = new Product
        {
            Manufacturer = Input.Manufacturer.Trim(),
            Model = Input.Model.Trim(),
            Price = Input.Price,
        };

        var appleId = new AppleId
        {
            Email = email,
            Password = Input.Password.Trim(),
            Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim(),
            ProductNavigation = product,
        };

        if (!appleIdDataService.Add(appleId))
        {
            Message = "The Apple ID could not be saved.";
            return Page();
        }

        return RedirectToPage("/Products/Details", new { id = appleId.Id, type = "appleid" });
    }

}
