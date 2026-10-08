namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the account and credential operations for the Account area.</summary>
public class AccountDataService(
    IBaseRepo<User> users,
    IBaseRepo<Person> people,
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

    /// <summary>
    /// Matches a username case-insensitively, mirroring the previous repository lookup.
    /// </summary>
    private static Expression<Func<User, bool>> UsernameEquals(string username)
        => user => user.Username.ToLower() == username.ToLower();

    /// <inheritdoc />
    public void EnsureAdminUser()
    {
        var admin = users.Find(UsernameEquals(DefaultAdminUsername));
        if (admin is null)
        {
            if (users.Any())
                return;

            var person = people.FindAll().FirstOrDefault() ?? new Person
            {
                FirstName = "Shop",
                LastName = "Administrator",
                PhoneNumber = "0000000000"
            };
            if (person.Id == 0)
                people.Add(person);

            admin = new User
            {
                Username = DefaultAdminUsername,
                PasswordHash = passwordHasher.Hash(DefaultAdminPassword),
                PersonId = person.Id
            };
            users.Add(admin);
            Logger.LogInformation("Created the initial admin account '{Username}'", DefaultAdminUsername);
            return;
        }

        if (admin.PasswordHash == "REPLACE_WITH_REAL_HASH")
        {
            admin.PasswordHash = passwordHasher.Hash(DefaultAdminPassword);
            users.Update(admin);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Returns only the username string so no <see cref="User"/> entity ever crosses onto a page.
    /// </remarks>
    public async Task<string?> GetAdminUsernameAsync()
        => await users.SelectFirstAsync(
            UsernameEquals(DefaultAdminUsername),
            user => user.Id,
            user => (string?)user.Username);

    /// <inheritdoc />
    public async Task<bool> ValidateCredentialsAsync(string username, string plainPassword)
    {
        var user = await users.FindAsync(UsernameEquals(username));
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
        var user = await users.FindAsync(UsernameEquals(username));
        if (user is null)
        {
            Logger.LogWarning("Password change failed for user '{Username}'", username);
            return false;
        }

        user.PasswordHash = passwordHasher.Hash(plainNewPassword);
        var updated = await users.UpdateAsync(user) > 0;
        if (updated)
            Logger.LogInformation("Password changed for user '{Username}'", username);
        else
            Logger.LogWarning("Password change failed for user '{Username}'", username);

        return updated;
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ChangeCredentialsAsync(
        string username,
        string currentPassword,
        string newUsername,
        string? newPassword)
    {
        var user = await users.FindAsync(UsernameEquals(username));
        if (user is null || !passwordHasher.Verify(user.PasswordHash, currentPassword))
            return new ServiceResult(false, "Invalid current password.");

        if (string.IsNullOrWhiteSpace(newUsername))
            return new ServiceResult(false, "Username is required.", "NewUsername");
        var normalizedUsername = newUsername.Trim();
        if (normalizedUsername.Length > 50)
            return new ServiceResult(false, "Username cannot exceed 50 characters.", "NewUsername");

        var existing = await users.FindAsync(UsernameEquals(normalizedUsername));
        if (existing is not null && existing.Id != user.Id)
            return new ServiceResult(false, "That username is already in use.", "NewUsername");

        user.Username = normalizedUsername;
        if (!string.IsNullOrWhiteSpace(newPassword))
            user.PasswordHash = passwordHasher.Hash(newPassword);

        if (await users.UpdateAsync(user) <= 0)
            return new ServiceResult(false, "The profile changes could not be saved.");

        Logger.LogInformation("Updated credentials for user '{Username}'", user.Username);
        return new ServiceResult(true, "Profile updated successfully.");
    }
}
