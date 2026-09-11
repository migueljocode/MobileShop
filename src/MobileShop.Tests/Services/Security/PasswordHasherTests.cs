namespace MobileShop.Tests.Services.Security;

public class PasswordHasherTests
{
    private readonly IPasswordHasher _hasher = new PasswordHasher();

    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = _hasher.Hash("MySecret123!");

        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    [Fact]
    public void Hash_SamePassword_ProducesDifferentHashes()
    {
        // Argon2 is salted → two hashes of the same password must differ
        var hash1 = _hasher.Hash("MySecret123!");
        var hash2 = _hasher.Hash("MySecret123!");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_ReturnsTrue_WhenPasswordMatches()
    {
        const string password = "MySecret123!";
        var hash = _hasher.Hash(password);

        Assert.True(_hasher.Verify(hash, password));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenPasswordDoesNotMatch()
    {
        var hash = _hasher.Hash("MySecret123!");

        Assert.False(_hasher.Verify(hash, "WrongPassword"));
    }

    [Fact]
    public void Verify_ReturnsFalse_WhenHashIsGarbage()
    {
        Assert.False(_hasher.Verify("not-a-real-hash", "anything"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("short")]
    [InlineData("A_Very_Long_Password_With_Numbers_1234567890_And_Symbols_!@#$%")]
    public void HashAndVerify_RoundTrip_Works_ForVariousPasswords(string password)
    {
        var hash = _hasher.Hash(password);

        Assert.True(_hasher.Verify(hash, password));
    }
}