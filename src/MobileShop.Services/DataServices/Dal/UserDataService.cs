namespace MobileShop.Services.DataServices.Dal;

public class UserDataService(
    IUserRepo userRepo,
    IPasswordHasher passwordHasher,
    ILogger<UserDataService> logger)
    : DataServiceBase<UserDataService, User>(userRepo, logger), IUserDataService
{
    private readonly IUserRepo _userRepo = userRepo;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    /// <inheritdoc />
    public User? FindByUsername(string username)
        => _userRepo.FindByUsername(username);

    /// <inheritdoc />
    public Task<User?> FindByUsernameAsync(string username)
        => _userRepo.FindByUsernameAsync(username);

    /// <inheritdoc />
    public bool ValidateCredentials(string username, string plainPassword)
    {
        var user = _userRepo.FindByUsername(username);
        if (user is null)
        {
            Logger.LogWarning("Login failed: user '{Username}' not found", username);
            return false;
        }

        var ok = _passwordHasher.Verify(user.PasswordHash, plainPassword);
        if (!ok)
            Logger.LogWarning("Login failed: invalid password for '{Username}'", username);
        else
            Logger.LogInformation("Login succeeded for '{Username}'", username);

        return ok;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateCredentialsAsync(string username, string plainPassword)
    {
        var user = await _userRepo.FindByUsernameAsync(username);
        if (user is null)
        {
            Logger.LogWarning("Login failed: user '{Username}' not found", username);
            return false;
        }

        var ok = _passwordHasher.Verify(user.PasswordHash, plainPassword);
        if (!ok)
            Logger.LogWarning("Login failed: invalid password for '{Username}'", username);
        else
            Logger.LogInformation("Login succeeded for '{Username}'", username);

        return ok;
    }

    /// <inheritdoc />
    public bool ChangePassword(int userId, string plainNewPassword)
    {
        var hash = _passwordHasher.Hash(plainNewPassword);
        var ok = _userRepo.ChangePassword(userId, hash);
        if (ok)
            Logger.LogInformation("Password changed for user Id={UserId}", userId);
        else
            Logger.LogWarning("Password change failed for user Id={UserId}", userId);
        return ok;
    }

    /// <inheritdoc />
    public bool ChangePassword(string username, string plainNewPassword)
    {
        var hash = _passwordHasher.Hash(plainNewPassword);
        var ok = _userRepo.ChangePassword(username, hash);
        if (ok)
            Logger.LogInformation("Password changed for user '{Username}'", username);
        else
            Logger.LogWarning("Password change failed for user '{Username}'", username);
        return ok;
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(int userId, string plainNewPassword)
    {
        var hash = _passwordHasher.Hash(plainNewPassword);
        var ok = await _userRepo.ChangePasswordAsync(userId, hash);
        if (ok)
            Logger.LogInformation("Password changed for user Id={UserId}", userId);
        else
            Logger.LogWarning("Password change failed for user Id={UserId}", userId);
        return ok;
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(string username, string plainNewPassword)
    {
        var hash = _passwordHasher.Hash(plainNewPassword);
        var ok = await _userRepo.ChangePasswordAsync(username, hash);
        if (ok)
            Logger.LogInformation("Password changed for user '{Username}'", username);
        else
            Logger.LogWarning("Password change failed for user '{Username}'", username);
        return ok;
    }

    /// <inheritdoc />
    public bool Create(User user, string plainPassword)
    {
        user.PasswordHash = _passwordHasher.Hash(plainPassword);
        return Add(user);
    }

    /// <inheritdoc />
    public Task<bool> CreateAsync(User user, string plainPassword)
    {
        user.PasswordHash = _passwordHasher.Hash(plainPassword);
        return AddAsync(user);
    }
}