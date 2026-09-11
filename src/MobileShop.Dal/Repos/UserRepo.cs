namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IUserRepo" />
public class UserRepo(AppDbContext context) : BaseRepo<User>(context), IUserRepo
{
    /// <inheritdoc />
    public bool ChangePassword(string username, string newPassword)
    {
        var user = Find(x => x.Username.ToLower() == username.ToLower());

        if (user is null)
            return false;

        user.PasswordHash = newPassword;
        return Update(user) > 0;
    }

    /// <inheritdoc />
    public bool ChangePassword(int userId, string newPassword)
    {
        var user = Find(userId);

        if (user is null)
            return false;

        user.PasswordHash = newPassword;
        return Update(user) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
    {
        var user = await FindAsync(userId);

        if (user is null)
            return false;

        user.PasswordHash = newPassword;
        return await UpdateAsync(user) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(string username, string newPassword)
    {
        var user = await FindAsync(x => x.Username.ToLower() == username.ToLower());

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

    /// <inheritdoc />
    public bool IsPasswordValid(int userId, string password)
    {
        var user = Find(userId);
        return user is not null && user.PasswordHash == password;
    }

    /// <inheritdoc />
    public bool IsPasswordValid(string username, string password)
    {
        var user = FindByUsername(username);
        return user is not null && user.PasswordHash == password;
    }

    /// <inheritdoc />
    public async Task<bool> IsPasswordValidAsync(int userId, string password)
    {
        var user = await FindAsync(userId);
        return user is not null && user.PasswordHash == password;
    }

    /// <inheritdoc />
    public async Task<bool> IsPasswordValidAsync(string username, string password)
    {
        var user = await FindByUsernameAsync(username);
        return user is not null && user.PasswordHash == password;
    }
}