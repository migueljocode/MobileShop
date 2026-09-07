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
}
