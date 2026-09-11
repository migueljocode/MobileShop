using Isopoh.Cryptography.Argon2;

namespace MobileShop.Services.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
        => Argon2.Hash(password);

    public bool Verify(string hash, string password)
        => Argon2.Verify(hash, password);
}
