namespace MobileShop.Web.Pages.Account;

public class ProfileModel(IUserDataService userDataService) : PageModel
{
    private readonly IUserDataService _userDataService = userDataService;

    [BindProperty]
    [Required]
    public string? CurrentPassword { get; set; }

    [BindProperty]
    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters.")]
    public string? NewPassword { get; set; }

    [BindProperty]
    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Confirm password must be at least 6 characters.")]
    [Compare(nameof(NewPassword), ErrorMessage = "New passwords do not match.")]
    public string? ConfirmPassword { get; set; }

    public string? Message { get; set; }

    public string? Username { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _userDataService.FindByUsernameAsync("admin");
        if (user != null)
        {
            Username = user.Username;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var credsOk = await _userDataService.ValidateCredentialsAsync("admin", CurrentPassword ?? string.Empty);
        if (!credsOk)
        {
            ModelState.AddModelError(string.Empty, "Invalid current password.");
            return Page();
        }

        if (NewPassword != ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "New passwords do not match.");
            return Page();
        }

        var ok = await _userDataService.ChangePasswordAsync("admin", NewPassword!);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Failed to change password.");
            return Page();
        }

        Message = "Password changed successfully.";
        ModelState.Clear();
        CurrentPassword = null;
        NewPassword = null;
        ConfirmPassword = null;
        return Page();
    }
}