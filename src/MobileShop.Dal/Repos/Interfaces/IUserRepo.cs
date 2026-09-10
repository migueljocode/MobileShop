namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="User"/> entities (login accounts).</summary>
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

    /*
        passwords passed to this methods should be already hashed
        hash libraries will be included in MobileShop.Services
        or maybe we will use ASP.NET Core’s built-in PasswordHasher<User> (or BCrypt).
    */ 
    bool IsPasswordValid(int userId, string password);
    bool IsPasswordValid(string username, string password);
    Task<bool> IsPasswordValidAsync(int userId, string password);
    Task<bool> IsPasswordValidAsync(string username, string password);

    bool ChangePassword(string username, string newPassword);
    bool ChangePassword(int userId, string newPassword);
    Task<bool> ChangePasswordAsync(int userId, string newPassword);
    Task<bool> ChangePasswordAsync(string username, string newPassword);
}
