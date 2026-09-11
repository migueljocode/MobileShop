namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>
/// Repository for <see cref="User"/> entities (login accounts).
/// Passwords are never hashed in this layer - every method here treats <c>password</c>/
/// <c>newPassword</c> parameters as an already-final value to store or compare directly against
/// <see cref="User.PasswordHash"/>. Hashing (and re-hashing on login, if a salted scheme like
/// BCrypt or ASP.NET Core's <c>PasswordHasher&lt;User&gt;</c> is used) belongs in the Services
/// layer, which is expected to call these methods with the value it wants stored/compared as-is.
/// </summary>
public interface IUserRepo : IBaseRepo<User>
{
    /// <summary>Finds a user by their login username.</summary>
    /// <param name="username">The username to look up.</param>
    /// <returns>The matching user, or <see langword="null"/> if none is found.</returns>
    User? FindByUsername(string username);

    /// <summary>Asynchronous version of <see cref="FindByUsername"/>.</summary>
    /// <param name="username">The username to look up.</param>
    /// <returns>The matching user, or <see langword="null"/> if none is found.</returns>
    Task<User?> FindByUsernameAsync(string username);

    /// <summary>Overwrites a user's stored password value directly - no hashing happens here, see the type-level remarks.</summary>
    /// <param name="username">The username of the user whose password is being changed.</param>
    /// <param name="newPassword">The new value to store in <see cref="User.PasswordHash"/>.</param>
    /// <returns><see langword="true"/> if the user was found and updated.</returns>
    bool ChangePassword(string username, string newPassword);

    /// <summary>Overload of <see cref="ChangePassword(string, string)"/> that looks the user up by Id instead of username.</summary>
    /// <param name="userId">The Id of the user whose password is being changed.</param>
    /// <param name="newPassword">The new value to store in <see cref="User.PasswordHash"/>.</param>
    /// <returns><see langword="true"/> if the user was found and updated.</returns>
    bool ChangePassword(int userId, string newPassword);

    /// <summary>Asynchronous version of <see cref="ChangePassword(int, string)"/>.</summary>
    /// <param name="userId">The Id of the user whose password is being changed.</param>
    /// <param name="newPassword">The new value to store in <see cref="User.PasswordHash"/>.</param>
    /// <returns><see langword="true"/> if the user was found and updated.</returns>
    Task<bool> ChangePasswordAsync(int userId, string newPassword);

    /// <summary>Asynchronous version of <see cref="ChangePassword(string, string)"/>.</summary>
    /// <param name="username">The username of the user whose password is being changed.</param>
    /// <param name="newPassword">The new value to store in <see cref="User.PasswordHash"/>.</param>
    /// <returns><see langword="true"/> if the user was found and updated.</returns>
    Task<bool> ChangePasswordAsync(string username, string newPassword);
}