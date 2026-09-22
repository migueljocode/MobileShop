namespace MobileShop.Services.DataServices.Api;

public class ApiUserDataService : IUserDataService
{
    public void EnsureAdminUser()
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public User? FindByUsername(string username)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<User?> FindByUsernameAsync(string username)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public bool ValidateCredentials(string username, string plainPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<bool> ValidateCredentialsAsync(string username, string plainPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public bool ChangePassword(int userId, string plainNewPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public bool ChangePassword(string username, string plainNewPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<bool> ChangePasswordAsync(int userId, string plainNewPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<bool> ChangePasswordAsync(string username, string plainNewPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public bool Create(User user, string plainPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<bool> CreateAsync(User user, string plainPassword)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public IEnumerable<User> FindAll(System.Linq.Expressions.Expression<Func<User, bool>>? predicate = null)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<IEnumerable<User>> FindAllAsync(System.Linq.Expressions.Expression<Func<User, bool>>? predicate = null)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public User? Find(int id)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public User? Find(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<User?> FindAsync(int id)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<User?> FindAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public bool Add(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public bool Update(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public bool Delete(int id)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public bool Delete(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<bool> AddAsync(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<bool> UpdateAsync(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");

    public Task<bool> DeleteAsync(User entity)
        => throw new NotImplementedException("ApiUserDataService is not implemented yet.");
}
