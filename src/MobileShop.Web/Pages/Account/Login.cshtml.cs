namespace MobileShop.Web.Pages.Account;

public class LoginModel(IAccountDataService dataService) : PageModel
{
    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
    public string? Message { get; private set; }

    public void OnGet(string? returnUrl = null)
        => ReturnUrl = returnUrl ?? Url.Content("~/");

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!await dataService.ValidateCredentialsAsync(Username, Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        Message = "Credentials validated. Authentication is not enabled in this stage.";
        ModelState.Clear();
        return Page();
    }
}
