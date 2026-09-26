namespace MobileShop.Web.Pages.People;

public class CreateSellerModel(ISellerDataService sellerDataService) : PageModel
{
    [BindProperty] public CreateSellerInputModel Input { get; set; } = new();
    public string? Message { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var seller = new Seller
        {
            EntityType = Input.EntityType,
            PersonNavigation = new Person
            {
                FirstName = Input.FirstName.Trim(),
                LastName = Input.LastName.Trim(),
                PhoneNumber = Input.PhoneNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim(),
            },
        };

        if (!await sellerDataService.AddAsync(seller))
        {
            Message = "The seller could not be created.";
            return Page();
        }

        return RedirectToPage("/People/Sellers");
    }

}
