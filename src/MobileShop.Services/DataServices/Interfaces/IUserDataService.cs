namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>
/// Defines the public contract for IUserDataService.
/// </summary>
public interface IUserDataService : IDataService<User>
{
    User? FindByUsername(string username);
    Task<User?> FindByUsernameAsync(string username);

    bool ValidateCredentials(string username, string plainPassword);
    Task<bool> ValidateCredentialsAsync(string username, string plainPassword);

    bool ChangePassword(int userId, string plainNewPassword);
    bool ChangePassword(string username, string plainNewPassword);
    Task<bool> ChangePasswordAsync(int userId, string plainNewPassword);
    Task<bool> ChangePasswordAsync(string username, string plainNewPassword);

    /// <summary>Hashes <paramref name="plainPassword"/> then persists the user.</summary>
    bool Create(User user, string plainPassword);
    Task<bool> CreateAsync(User user, string plainPassword);
}
