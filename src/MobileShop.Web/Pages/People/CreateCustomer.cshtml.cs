namespace MobileShop.Web.Pages.People;

public class CreateCustomerModel(ICustomerDataService customerDataService) : PageModel
{
    [BindProperty] public CreateCustomerInputModel Input { get; set; } = new();
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

}
