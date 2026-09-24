namespace MobileShop.Web.Pages.Account;

public class ProfileModel : PageModel
{
    private readonly IUserDataService _userDataService;
    private readonly IPasswordHasher _passwordHasher;

    public ProfileModel(IUserDataService userDataService, IPasswordHasher passwordHasher)
    {
        _userDataService = userDataService;
        _passwordHasher = passwordHasher;
    }

    [BindProperty]
    public string? CurrentPassword { get; set; }

    [BindProperty]
    public string? NewPassword { get; set; }

    [BindProperty]
    public string? ConfirmPassword { get; set; }

    public string? Message { get; set; }

    public string? Username { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _userDataService.FindByUsernameAsync(User.Identity?.Name ?? string.Empty);
        if (user != null)
        {
            Username = user.Username;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await _userDataService.ValidateCredentialsAsync(Username ?? string.Empty, CurrentPassword ?? string.Empty))
        {
            ModelState.AddModelError(string.Empty, "Invalid current password.");
            return Page();
        }

        if (NewPassword != ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "New passwords do not match.");
            return Page();
        }

        _userDataService.ValidateCredentials(Username ?? string.Empty, CurrentPassword ?? string.Empty);
        ModelState.Clear();
        Message = "Password change requested. Authentication will enable the change.";
        return Page();
    }
}