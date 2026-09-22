namespace MobileShop.Services.DataServices.Api;

public class ApiUserDataService : IUserDataService
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

    /// <inheritdoc />
    public IEnumerable<User> FindAll(Expression<Func<User, bool>>? predicate = null)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<IEnumerable<User>> FindAllAsync(Expression<Func<User, bool>>? predicate = null)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public User? Find(int id)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public User? Find(Expression<Func<User, bool>> predicate)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<User?> FindAsync(int id)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<User?> FindAsync(Expression<Func<User, bool>> predicate)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Add(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Update(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(int id)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public bool Delete(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> AddAsync(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> UpdateAsync(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    /// <inheritdoc />
    public Task<bool> DeleteAsync(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");
}
