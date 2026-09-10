namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IUserRepo" />
public class UserRepo(AppDbContext context) : BaseRepo<User>(context), IUserRepo
{
    public bool ChangePassword(string username, string newPassword)
    {
        var user = Find(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        
        if (user is null)
            return false;

        user.PasswordHash = newPassword;
        return Update(user) > 0;
    }

    public bool ChangePassword(int userId, string newPassword)
    {
        var user = Find(userId);
        
        if (user is null)
            return false;

        user.PasswordHash = newPassword;
        return Update(user) > 0;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
    {
        var user = await FindAsync(userId);
        
        if (user is null)
            return false;

        user.PasswordHash = newPassword;
        return await UpdateAsync(user) > 0;
    }

    public async Task<bool> ChangePasswordAsync(string username, string newPassword)
    {
        var user = await FindAsync(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        
        if (user is null)
            return false;

        user.PasswordHash = newPassword;
        return await UpdateAsync(user) > 0;
    }

    // can be simply replaced with Find(x => x.username == username)
    /// <inheritdoc />
    public User? FindByUsername(string username)
        => Table.FirstOrDefault(u => u.Username == username);

    /// <inheritdoc />
    public async Task<User?> FindByUsernameAsync(string username)
        => await Table.FirstOrDefaultAsync(u => u.Username == username);


    // this methods can be used to validate password based on Password Attribute 
    // or just use PasswordHasher.VerifyHashedPassword()
    public bool IsPasswordValid(int userId, string password)
    {
        throw new NotImplementedException();
    }

    public bool IsPasswordValid(string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsPasswordValidAsync(int userId, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsPasswordValidAsync(string username, string password)
    {
        throw new NotImplementedException();
    }
}
