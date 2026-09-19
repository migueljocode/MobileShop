namespace MobileShop.Services.Initialization;

/// <summary>
/// Development-time seeding for the default admin account. This lives in the service layer
/// rather than in the Dal layer because it needs the password hasher, which only the service
/// layer knows about.
/// </summary>
public class AdminSeeder(IUserRepo userRepo, IPasswordHasher passwordHasher)
{
    /// <summary>The username of the seeded admin account.</summary>
    public const string DefaultUsername = "admin";

    /// <summary>The development password assigned to the seeded admin account.</summary>
    public const string DefaultPassword = "Admin@123";

    private readonly IUserRepo _userRepo = userRepo;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Replaces the admin account's stored password with a real hash of
    /// <see cref="DefaultPassword"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the admin account is missing, which means the sample data was not seeded.
    /// </exception>
    public void EnsureDefaultAdmin()
    {
        var admin = _userRepo.FindByUsername(DefaultUsername)
            ?? throw new InvalidOperationException(
                $"The default admin account '{DefaultUsername}' was not found - seed the sample data first.");

        admin.PasswordHash = _passwordHasher.Hash(DefaultPassword);
        _userRepo.Update(admin);
    }
}