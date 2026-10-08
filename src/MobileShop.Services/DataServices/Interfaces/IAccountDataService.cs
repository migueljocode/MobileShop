namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>Defines account and credential operations provided by the Account area.</summary>
public interface IAccountDataService
{
    /// <summary>Ensures the development admin account exists.</summary>
    void EnsureAdminUser();

    /// <summary>Gets the admin username asynchronously.</summary>
    Task<string?> GetAdminUsernameAsync();

    /// <summary>Validates a username and plain-text password.</summary>
    /// <param name="username">The username.</param>
    /// <param name="plainPassword">The plain-text password.</param>
    Task<bool> ValidateCredentialsAsync(string username, string plainPassword);

    /// <summary>Changes a user's plain-text password.</summary>
    /// <param name="username">The username.</param>
    /// <param name="plainNewPassword">The new plain-text password.</param>
    Task<bool> ChangePasswordAsync(string username, string plainNewPassword);

    /// <summary>Validates the current password and updates the username and optional password together.</summary>
    Task<ServiceResult> ChangeCredentialsAsync(
        string username,
        string currentPassword,
        string newUsername,
        string? newPassword);
}
