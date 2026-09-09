namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IUserRepo" />
public class UserRepo(AppDbContext context) : BaseRepo<User>(context), IUserRepo
{
    public bool ChangePassword(string newPassword)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangePasswordAsync(string newPassword)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public User? FindByUsername(string username)
        => Table.FirstOrDefault(u => u.Username == username);

    /// <inheritdoc />
    public async Task<User?> FindByUsernameAsync(string username)
        => await Table.FirstOrDefaultAsync(u => u.Username == username);

    public bool IsPasswordValid(string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsPasswordValidAsync(string password)
    {
        throw new NotImplementedException();
    }
}
