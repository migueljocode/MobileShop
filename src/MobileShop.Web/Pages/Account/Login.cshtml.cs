namespace MobileShop.Web.Pages.Account;

public class LoginModel(IAccountDataService dataService) : PageModel
{
    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
    public string? Message { get; private set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect(Url.Content("~/"));
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!await dataService.ValidateCredentialsAsync(Username, Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Username!),
            new Claim(ClaimTypes.Name, Username!)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return LocalRedirect(ReturnUrl!);
    }
}
