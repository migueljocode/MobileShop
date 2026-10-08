namespace MobileShop.Web.Pages.Account;

public class ProfileModel(IAccountDataService dataService) : PageModel
{
    [BindProperty]
    [Required]
    [StringLength(50)]
    public string? NewUsername { get; set; }

    [BindProperty]
    [Required]
    public string? CurrentPassword { get; set; }

    [BindProperty]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters.")]
    public string? NewPassword { get; set; }

    [BindProperty]
    [Compare(nameof(NewPassword), ErrorMessage = "New passwords do not match.")]
    public string? ConfirmPassword { get; set; }

    public string? Message { get; set; }

    public string? Username { get; set; }

    public void OnGet()
    {
        Username = User.Identity?.Name;
        NewUsername = Username;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Username = User.Identity?.Name;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            return Challenge();
        }

        var result = await dataService.ChangeCredentialsAsync(
            Username, CurrentPassword!, NewUsername!, NewPassword);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(result.ErrorField ?? string.Empty, result.Message ?? "The profile changes could not be saved.");
            return Page();
        }

        Username = NewUsername!.Trim();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Username),
            new Claim(ClaimTypes.Name, Username)
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

        Message = result.Message;
        ModelState.Clear();
        NewUsername = Username;
        CurrentPassword = null;
        NewPassword = null;
        ConfirmPassword = null;
        return Page();
    }
}
