namespace MobileShop.Services.DataServices.Api;

public class ApiAccountDataService : IAccountDataService
{
    /// <inheritdoc />
    public void EnsureAdminUser()
        => throw new NotImplementedException("ApiAccountDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<string?> GetAdminUsernameAsync()
        => throw new NotImplementedException("ApiAccountDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> ValidateCredentialsAsync(string username, string plainPassword)
        => throw new NotImplementedException("ApiAccountDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> ChangePasswordAsync(string username, string plainNewPassword)
        => throw new NotImplementedException("ApiAccountDataService is not implemented yet.");

    public Task<ServiceResult> ChangeCredentialsAsync(string username, string currentPassword, string newUsername, string? newPassword)
        => throw new NotImplementedException("ApiAccountDataService is not implemented yet.");
}
