namespace MobileShop.Tests.Services.DataServices.Dal;

public class AccountDataServiceTests : RepoTestBase
{
    private readonly AccountDataService _service;
        private readonly IPasswordHasher _hasher = new ArgonPasswordHasher();

    public AccountDataServiceTests()
    {
        _service = new AccountDataService(
            new BaseRepo<User>(Context),
            _hasher,
            NullLogger<AccountDataService>.Instance);
    }

    private User AddUser(string username, string password)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = _hasher.Hash(password),
        };
        Context.Users.Add(user);
        Context.SaveChanges();
        return user;
    }

    [Fact]
    public async Task GetAdminUsernameAsync_returns_null_when_admin_missing()
    {
        Assert.Null(await _service.GetAdminUsernameAsync());
    }

    [Fact]
    public async Task GetAdminUsernameAsync_returns_username_when_admin_seeded()
    {
        AddUser(AccountDataService.DefaultAdminUsername, "whatever");

        var username = await _service.GetAdminUsernameAsync();

        Assert.Equal("admin", username);
    }

    [Fact]
    public async Task EnsureAdminUser_sets_a_hash_that_verifies_against_the_default_password()
    {
        AddUser(AccountDataService.DefaultAdminUsername, "stale-password");

        _service.EnsureAdminUser();

        var admin = Context.Users.Single(u => u.Username == AccountDataService.DefaultAdminUsername);
        Assert.NotEqual("stale-password", admin.PasswordHash);
        Assert.NotEmpty(admin.PasswordHash);
        Assert.True(_hasher.Verify(admin.PasswordHash, AccountDataService.DefaultAdminPassword));
        Assert.True(await _service.ValidateCredentialsAsync("admin", AccountDataService.DefaultAdminPassword));
    }

    [Fact]
    public void EnsureAdminUser_throws_when_the_admin_account_was_not_seeded()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => _service.EnsureAdminUser());

        Assert.Contains(AccountDataService.DefaultAdminUsername, exception.Message);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_accepts_the_correct_password()
    {
        AddUser("admin", "Admin@123");

        Assert.True(await _service.ValidateCredentialsAsync("admin", "Admin@123"));
    }

    [Fact]
    public async Task ValidateCredentialsAsync_rejects_wrong_password_and_unknown_user()
    {
        AddUser("admin", "Admin@123");

        Assert.False(await _service.ValidateCredentialsAsync("admin", "wrong"));
        Assert.False(await _service.ValidateCredentialsAsync("nobody", "Admin@123"));
    }

    [Fact]
    public async Task ChangePasswordAsync_stores_a_verifiable_hash_and_the_new_password_validates()
    {
        AddUser("admin", "Admin@123");

        var changed = await _service.ChangePasswordAsync("admin", "New@456");

        Assert.True(changed);
        var admin = Context.Users.Single(u => u.Username == "admin");
        Assert.NotEqual(AccountDataService.DefaultAdminPassword, admin.PasswordHash);
        Assert.True(_hasher.Verify(admin.PasswordHash, "New@456"));
        Assert.True(await _service.ValidateCredentialsAsync("admin", "New@456"));
        Assert.False(await _service.ValidateCredentialsAsync("admin", AccountDataService.DefaultAdminPassword));
    }

    [Fact]
    public async Task ChangePasswordAsync_returns_false_for_unknown_user()
    {
        Assert.False(await _service.ChangePasswordAsync("nobody", "New@456"));
    }

    [Fact]
    public async Task Credential_lookups_are_case_insensitive_in_both_directions()
    {
        AddUser("Admin", "Admin@123");

        // Stored as "Admin" but reached with different casing, as the previous repository lookup allowed.
        Assert.Equal("Admin", await _service.GetAdminUsernameAsync());
        Assert.True(await _service.ValidateCredentialsAsync("admin", "Admin@123"));
        Assert.True(await _service.ChangePasswordAsync("ADMIN", "New@456"));
        Assert.True(await _service.ValidateCredentialsAsync("aDmIn", "New@456"));
    }
}
