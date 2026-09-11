namespace MobileShop.Tests.Services.Security;

public class PasswordHasherUserRepoIntegrationTests : RepoTestBase
{
    private readonly IPasswordHasher _hasher = new PasswordHasher();
    private readonly IUserRepo _userRepo;

    public PasswordHasherUserRepoIntegrationTests()
    {
        _userRepo = new UserRepo(Context);
    }

    private User SeedUser(string username, string plainPassword)
    {
        var person = new Person
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "09120000000"
        };
        Context.People.Add(person);
        Context.SaveChanges();

        var user = new User
        {
            PersonId = person.Id,
            Username = username,
            PasswordHash = _hasher.Hash(plainPassword)
        };

        _userRepo.Add(user);
        return user;
    }

    [Fact]
    public void LoginFlow_Succeeds_WithCorrectPassword()
    {
        const string username = "alice";
        const string password = "CorrectHorseBatteryStaple";

        var user = SeedUser(username, password);

        var stored = _userRepo.FindByUsername(username);
        Assert.NotNull(stored);

        Assert.True(_hasher.Verify(stored!.PasswordHash, password));
    }

    [Fact]
    public void LoginFlow_Fails_WithWrongPassword()
    {
        const string username = "bob";
        const string password = "CorrectHorseBatteryStaple";

        SeedUser(username, password);

        var stored = _userRepo.FindByUsername(username);
        Assert.NotNull(stored);

        Assert.False(_hasher.Verify(stored!.PasswordHash, "WrongPassword"));
    }

    [Fact]
    public void ChangePassword_ThenLogin_WithNewPassword_Succeeds()
    {
        const string username = "carol";
        const string oldPassword = "OldPassword123!";
        const string newPassword = "NewPassword456!";

        var user = SeedUser(username, oldPassword);

        // Services layer would hash, then call repo
        var newHash = _hasher.Hash(newPassword);
        var changed = _userRepo.ChangePassword(user.Id, newHash);

        Assert.True(changed);

        var stored = _userRepo.Find(user.Id);
        Assert.NotNull(stored);

        // old password no longer works
        Assert.False(_hasher.Verify(stored!.PasswordHash, oldPassword));

        // new password works
        Assert.True(_hasher.Verify(stored.PasswordHash, newPassword));
    }

    [Fact]
    public void ChangePassword_ByUsername_CaseInsensitive_Works()
    {
        const string username = "Dave";
        const string password = "InitialPass!";
        const string newPassword = "UpdatedPass!";

        SeedUser(username, password);

        var newHash = _hasher.Hash(newPassword);
        var changed = _userRepo.ChangePassword(username.ToUpperInvariant(), newHash);

        Assert.True(changed);

        var stored = _userRepo.FindByUsername(username);
        Assert.NotNull(stored);
        Assert.True(_hasher.Verify(stored!.PasswordHash, newPassword));
    }

    [Fact]
    public async Task ChangePasswordAsync_ThenLogin_Works()
    {
        const string username = "eve";
        const string oldPassword = "AsyncOld!";
        const string newPassword = "AsyncNew!";

        var user = SeedUser(username, oldPassword);

        var newHash = _hasher.Hash(newPassword);
        var changed = await _userRepo.ChangePasswordAsync(user.Id, newHash);

        Assert.True(changed);

        var stored = await _userRepo.FindAsync(user.Id);
        Assert.NotNull(stored);
        Assert.True(_hasher.Verify(stored!.PasswordHash, newPassword));
        Assert.False(_hasher.Verify(stored.PasswordHash, oldPassword));
    }
}