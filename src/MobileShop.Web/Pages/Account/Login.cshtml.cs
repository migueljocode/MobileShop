namespace MobileShop.Web.Pages.Account;

[AllowAnonymous]
public class LoginModel(IUserDataService userDataService) : PageModel
{
    private readonly IUserDataService _userDataService = userDataService;

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
        => ReturnUrl = returnUrl ?? Url.Content("~/");

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = _userDataService.FindByUsername(Username);
        if (user is null || !_userDataService.ValidateCredentials(Username, Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return LocalRedirect(ReturnUrl);
    }
}
