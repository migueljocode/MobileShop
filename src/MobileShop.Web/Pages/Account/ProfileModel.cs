namespace MobileShop.Web.Pages.Account;

public class ProfileModel(IAccountDataService dataService) : PageModel
{
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
        => Username = await dataService.GetAdminUsernameAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // The service owns the admin username; "admin" is only a last-resort fallback so a
// missing seed surfaces as a normal failed validation rather than a null dereference.
        var adminUsername = await dataService.GetAdminUsernameAsync() ?? "admin";

        var credsOk = await dataService.ValidateCredentialsAsync(adminUsername, CurrentPassword ?? string.Empty);
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

        var ok = await dataService.ChangePasswordAsync(adminUsername, NewPassword!);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "Failed to change password.");
            return Page();
        }

        Username = adminUsername;
        Message = "Password changed successfully.";
        ModelState.Clear();
        CurrentPassword = null;
        NewPassword = null;
        ConfirmPassword = null;
        return Page();
    }
}
