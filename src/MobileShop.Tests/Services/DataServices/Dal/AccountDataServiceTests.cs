namespace MobileShop.Tests.Services.DataServices.Dal;

public class AccountDataServiceTests : RepoTestBase
{
    private readonly AccountDataService _service;
        private readonly IPasswordHasher _hasher = new ArgonPasswordHasher();

    public AccountDataServiceTests()
    {
        _service = new AccountDataService(
            new BaseRepo<User>(Context),
            new BaseRepo<Person>(Context),
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
    public async Task EnsureAdminUser_does_not_reset_an_existing_password()
    {
        AddUser(AccountDataService.DefaultAdminUsername, "stale-password");

        _service.EnsureAdminUser();

        var admin = Context.Users.Single(u => u.Username == AccountDataService.DefaultAdminUsername);
        Assert.NotEqual("stale-password", admin.PasswordHash);
        Assert.NotEmpty(admin.PasswordHash);
        Assert.True(_hasher.Verify(admin.PasswordHash, "stale-password"));
        Assert.True(await _service.ValidateCredentialsAsync("admin", "stale-password"));
    }

    [Fact]
    public async Task EnsureAdminUser_creates_the_initial_admin_when_no_user_exists()
    {
        _service.EnsureAdminUser();

        var admin = Assert.Single(Context.Users);
        Assert.Equal(AccountDataService.DefaultAdminUsername, admin.Username);
        Assert.True(await _service.ValidateCredentialsAsync(admin.Username, AccountDataService.DefaultAdminPassword));
        Assert.NotNull(Context.People.Find(admin.PersonId));
    }

    [Fact]
    public async Task EnsureAdminUser_repairs_the_seed_placeholder_hash()
    {
        var admin = AddUser(AccountDataService.DefaultAdminUsername, "temporary-password");
        admin.PasswordHash = "REPLACE_WITH_REAL_HASH";
        Context.SaveChanges();

        _service.EnsureAdminUser();

        Assert.True(await _service.ValidateCredentialsAsync(
            AccountDataService.DefaultAdminUsername, AccountDataService.DefaultAdminPassword));
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
    public async Task ChangeCredentialsAsync_updates_username_and_password_after_current_password_check()
    {
        AddUser("admin", "Admin@123");

        var result = await _service.ChangeCredentialsAsync("admin", "Admin@123", "shop-owner", "New@456");

        Assert.True(result.Succeeded);
        Assert.True(await _service.ValidateCredentialsAsync("shop-owner", "New@456"));
        Assert.False(await _service.ValidateCredentialsAsync("admin", "Admin@123"));
    }

    [Fact]
    public async Task ChangeCredentialsAsync_can_change_only_the_username()
    {
        AddUser("admin", "Admin@123");

        var result = await _service.ChangeCredentialsAsync("admin", "Admin@123", "shop-owner", null);

        Assert.True(result.Succeeded);
        Assert.True(await _service.ValidateCredentialsAsync("shop-owner", "Admin@123"));
    }

    [Fact]
    public async Task ChangeCredentialsAsync_rejects_incorrect_current_password_without_updates()
    {
        AddUser("admin", "Admin@123");

        var result = await _service.ChangeCredentialsAsync("admin", "wrong", "shop-owner", "New@456");

        Assert.False(result.Succeeded);
        Assert.True(await _service.ValidateCredentialsAsync("admin", "Admin@123"));
        Assert.False(await _service.ValidateCredentialsAsync("shop-owner", "New@456"));
    }

    [Fact]
    public async Task ChangeCredentialsAsync_rejects_duplicate_username()
    {
        AddUser("admin", "Admin@123");
        AddUser("other", "Other@123");

        var result = await _service.ChangeCredentialsAsync("admin", "Admin@123", "other", null);

        Assert.False(result.Succeeded);
        Assert.Equal("NewUsername", result.ErrorField);
        Assert.True(await _service.ValidateCredentialsAsync("admin", "Admin@123"));
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
