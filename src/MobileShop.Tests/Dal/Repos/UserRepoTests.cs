namespace MobileShop.Tests.Dal.Repos;

public class UserRepoTests : BaseRepoTests<User, IUserRepo>
{
    protected override IUserRepo CreateRepo() => new UserRepo(Context);

    protected override User CreateValidEntity()
    {
        var person = new Person { FirstName = "Test", LastName = "User", PhoneNumber = "09120000000" };
        Context.People.Add(person);
        Context.SaveChanges();

        // Username is unique - randomize so multiple calls (e.g. in FindAll tests) don't collide
        return new User { PersonId = person.Id, Username = $"user_{Guid.NewGuid():N}", PasswordHash = "hashed" };
    }

    [Fact]
    public void FindByUsername_ReturnsUser_WhenUsernameMatches()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        repo.Add(user);

        var found = repo.FindByUsername(user.Username);

        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

    [Fact]
    public void FindByUsername_ReturnsNull_WhenNoMatch()
        => Assert.Null(CreateRepo().FindByUsername("does-not-exist"));

    [Fact]
    public async Task FindByUsernameAsync_ReturnsUser_WhenUsernameMatches()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        await repo.AddAsync(user);

        var found = await repo.FindByUsernameAsync(user.Username);

        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

    [Fact]
    public async Task FindByUsernameAsync_ReturnsNull_WhenNoMatch()
        => Assert.Null(await CreateRepo().FindByUsernameAsync("does-not-exist"));

    [Fact]
    public void ChangePassword_ById_UpdatesHash()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        repo.Add(user);

        var result = repo.ChangePassword(user.Id, "new-hash");

        Assert.True(result);
        Assert.Equal("new-hash", repo.Find(user.Id)!.PasswordHash);
    }

    [Fact]
    public void ChangePassword_ById_ReturnsFalse_WhenUserNotFound()
        => Assert.False(CreateRepo().ChangePassword(-1, "new-hash"));

    [Fact]
    public void ChangePassword_ByUsername_UpdatesHash_CaseInsensitively()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        repo.Add(user);

        var result = repo.ChangePassword(user.Username.ToUpperInvariant(), "new-hash");

        Assert.True(result);
        Assert.Equal("new-hash", repo.Find(user.Id)!.PasswordHash);
    }

    [Fact]
    public void ChangePassword_ByUsername_ReturnsFalse_WhenUserNotFound()
        => Assert.False(CreateRepo().ChangePassword("does-not-exist", "new-hash"));

    [Fact]
    public async Task ChangePasswordAsync_ById_UpdatesHash()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        await repo.AddAsync(user);

        var result = await repo.ChangePasswordAsync(user.Id, "new-hash");

        Assert.True(result);
        Assert.Equal("new-hash", (await repo.FindAsync(user.Id))!.PasswordHash);
    }

    [Fact]
    public async Task ChangePasswordAsync_ByUsername_UpdatesHash_CaseInsensitively()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        await repo.AddAsync(user);

        var result = await repo.ChangePasswordAsync(user.Username.ToUpperInvariant(), "new-hash");

        Assert.True(result);
        Assert.Equal("new-hash", (await repo.FindAsync(user.Id))!.PasswordHash);
    }

    /* 
    // they are considered deprecated and should be replaced with proper implementation of IsPasswordValid.
    [Fact]
    public void IsPasswordValid_ById_ReturnsTrue_WhenHashMatches()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        repo.Add(user);

        Assert.True(repo.IsPasswordValid(user.Id, user.PasswordHash));
    }

    [Fact]
    public void IsPasswordValid_ById_ReturnsFalse_WhenHashDoesNotMatch()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        repo.Add(user);

        Assert.False(repo.IsPasswordValid(user.Id, "wrong-hash"));
    }

    [Fact]
    public void IsPasswordValid_ByUsername_ReturnsTrue_WhenHashMatches()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        repo.Add(user);

        Assert.True(repo.IsPasswordValid(user.Username, user.PasswordHash));
    }

    [Fact]
    public void IsPasswordValid_ByUsername_ReturnsFalse_WhenUserNotFound()
        => Assert.False(CreateRepo().IsPasswordValid("does-not-exist", "anything"));

    [Fact]
    public async Task IsPasswordValidAsync_ById_ReturnsTrue_WhenHashMatches()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        await repo.AddAsync(user);

        Assert.True(await repo.IsPasswordValidAsync(user.Id, user.PasswordHash));
    }

    [Fact]
    public async Task IsPasswordValidAsync_ByUsername_ReturnsFalse_WhenHashDoesNotMatch()
    {
        var repo = CreateRepo();
        var user = CreateValidEntity();
        await repo.AddAsync(user);

        Assert.False(await repo.IsPasswordValidAsync(user.Username, "wrong-hash"));
    }
    */
}