namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the account and credential operations for the Account area.</summary>
public class AccountDataService(
    IUserRepo users,
    IPasswordHasher passwordHasher,
    ILogger<AccountDataService> logger)
    : IAccountDataService
{
    /// <summary>The username of the seeded admin account.</summary>
    public const string DefaultAdminUsername = "admin";

    /// <summary>The development password assigned to the seeded admin account.</summary>
    public const string DefaultAdminPassword = "Admin@123";

    /// <summary>Gets the structured logger for this account service.</summary>
    protected ILogger<AccountDataService> Logger { get; } = logger;

    /// <inheritdoc />
    public void EnsureAdminUser()
    {
        var admin = users.FindByUsername(DefaultAdminUsername)
            ?? throw new InvalidOperationException(
                $"The default admin account '{DefaultAdminUsername}' was not found - seed the sample data first.");

        admin.PasswordHash = passwordHasher.Hash(DefaultAdminPassword);
        users.Update(admin);

        Logger.LogInformation("Ensured the default admin account '{Username}'", DefaultAdminUsername);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Returns only the username string so no <see cref="User"/> entity ever crosses onto a page.
    /// </remarks>
    public async Task<string?> GetAdminUsernameAsync()
    {
        var admin = await users.FindByUsernameAsync(DefaultAdminUsername);
        return admin?.Username;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateCredentialsAsync(string username, string plainPassword)
    {
        var user = await users.FindByUsernameAsync(username);
        if (user is null)
        {
            Logger.LogWarning("Login failed: user '{Username}' not found", username);
            return false;
        }

        var ok = passwordHasher.Verify(user.PasswordHash, plainPassword);
        if (!ok)
            Logger.LogWarning("Login failed: invalid password for '{Username}'", username);
        else
            Logger.LogInformation("Login succeeded for '{Username}'", username);

        return ok;
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(string username, string plainNewPassword)
    {
        var hash = passwordHasher.Hash(plainNewPassword);
        var ok = await users.ChangePasswordAsync(username, hash);
        if (ok)
            Logger.LogInformation("Password changed for user '{Username}'", username);
        else
            Logger.LogWarning("Password change failed for user '{Username}'", username);

        return ok;
    }
}
