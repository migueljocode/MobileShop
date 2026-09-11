namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IUserRepo" />
public class UserRepo(AppDbContext context) : BaseRepo<User>(context), IUserRepo
{
    /// <inheritdoc />
    public User? FindByUsername(string username)
        => Table.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());

    /// <inheritdoc />
    public async Task<User?> FindByUsernameAsync(string username)
        => await Table.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

    /// <inheritdoc />
    public bool ChangePassword(string username, string newPassword)
    {
        var user = FindByUsername(username);
        if (user is null) return false;

        user.PasswordHash = newPassword;
        return Update(user) > 0;
    }

    /// <inheritdoc />
    public bool ChangePassword(int userId, string newPassword)
    {
        var user = Find(userId);
        if (user is null) return false;

        user.PasswordHash = newPassword;
        return Update(user) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
    {
        var user = await FindAsync(userId);
        if (user is null) return false;

        user.PasswordHash = newPassword;
        return await UpdateAsync(user) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(string username, string newPassword)
    {
        var user = await FindByUsernameAsync(username);
        if (user is null) return false;

        user.PasswordHash = newPassword;
        return await UpdateAsync(user) > 0;
    }
}