namespace MobileShop.Web.Pages.People;

public class CreateCustomerModel(ICustomerDataService customerDataService) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public string? Message { get; private set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var customer = new Customer
        {
            NationalId = Input.NationalId.Trim(),
            PersonNavigation = new Person
            {
                FirstName = Input.FirstName.Trim(),
                LastName = Input.LastName.Trim(),
                PhoneNumber = Input.PhoneNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim(),
            },
        };

        if (!customerDataService.Add(customer))
        {
            Message = "The customer could not be created.";
            return Page();
        }

        return RedirectToPage("/People/Customers");
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
        [StringLength(10)]
        public string NationalId { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
