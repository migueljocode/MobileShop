namespace MobileShop.Services.DataServices.Api;

public class ApiUserDataService : ApiDataServiceBase<User>, IUserDataService
{
    /// <inheritdoc />
    public void EnsureAdminUser()
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public User? FindByUsername(string username)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<User?> FindByUsernameAsync(string username)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public bool ValidateCredentials(string username, string plainPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> ValidateCredentialsAsync(string username, string plainPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public bool ChangePassword(int userId, string plainNewPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public bool ChangePassword(string username, string plainNewPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> ChangePasswordAsync(int userId, string plainNewPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> ChangePasswordAsync(string username, string plainNewPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Create(User user, string plainPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> CreateAsync(User user, string plainPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");
}
