namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for IUserDataService.
/// </summary>
public interface IUserDataService : IDataService<User>
{
    /// <summary>Finds a user by username.</summary>
    /// <param name="username">The username.</param>
    /// <returns>The user, or <see langword="null"/> when not found.</returns>
    User? FindByUsername(string username);
    /// <summary>Finds a user by username asynchronously.</summary>
    /// <param name="username">The username.</param>
    /// <returns>The user, or <see langword="null"/> when not found.</returns>
    Task<User?> FindByUsernameAsync(string username);

    /// <summary>Validates a username and plain-text password.</summary>
    /// <param name="username">The username.</param>
    /// <param name="plainPassword">The plain-text password.</param>
    /// <returns><see langword="true"/> when the credentials are valid.</returns>
    bool ValidateCredentials(string username, string plainPassword);
    /// <summary>Validates credentials asynchronously.</summary>
    /// <param name="username">The username.</param>
    /// <param name="plainPassword">The plain-text password.</param>
    /// <returns><see langword="true"/> when the credentials are valid.</returns>
    Task<bool> ValidateCredentialsAsync(string username, string plainPassword);

    /// <summary>Changes a user's password by identifier.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="plainNewPassword">The new plain-text password.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    bool ChangePassword(int userId, string plainNewPassword);
    /// <summary>Changes a user's password by username.</summary>
    /// <param name="username">The username.</param>
    /// <param name="plainNewPassword">The new plain-text password.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    bool ChangePassword(string username, string plainNewPassword);
    /// <summary>Changes a user's password by identifier asynchronously.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="plainNewPassword">The new plain-text password.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    Task<bool> ChangePasswordAsync(int userId, string plainNewPassword);
    /// <summary>Changes a user's password by username asynchronously.</summary>
    /// <param name="username">The username.</param>
    /// <param name="plainNewPassword">The new plain-text password.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    Task<bool> ChangePasswordAsync(string username, string plainNewPassword);

    /// <summary>Hashes <paramref name="plainPassword"/> then persists the user.</summary>
    /// <param name="user">The user to create.</param>
    /// <param name="plainPassword">The user's plain-text password.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    bool Create(User user, string plainPassword);
    /// <summary>Hashes the password then persists the user asynchronously.</summary>
    /// <param name="user">The user to create.</param>
    /// <param name="plainPassword">The user's plain-text password.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    Task<bool> CreateAsync(User user, string plainPassword);
}
