namespace MobileShop.Web.Pages.People;

public class CreateSellerModel(ISellerDataService sellerDataService) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public string? Message { get; private set; }

    public IActionResult OnPost()
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

        if (!sellerDataService.Add(seller))
        {
            Message = "The seller could not be created.";
            return Page();
        }

        return RedirectToPage("/People/Sellers");
    }

    public sealed class InputModel
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public SellerEntityType EntityType { get; set; } = SellerEntityType.Real;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
