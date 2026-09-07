namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IUserRepo" />
public class UserRepo(AppDbContext context) : BaseRepo<User>(context), IUserRepo
{
    /// <inheritdoc />
    public User? FindByUsername(string username)
        => Table.FirstOrDefault(u => u.Username == username);

    /// <inheritdoc />
    public async Task<User?> FindByUsernameAsync(string username)
        => await Table.FirstOrDefaultAsync(u => u.Username == username);
}
