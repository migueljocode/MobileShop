namespace MobileShop.Tests.Dal.Repos;

public class UserRepoTests : BaseRepoTests<User, IUserRepo>
{
    protected override IUserRepo CreateRepo() => new UserRepo(Context);

    protected override User CreateValidEntity()
    {
        var person = new Person { FirstName = "Test", LastName = "User", PhoneNumber = "09120000000" };
        Context.People.Add(person);
        Context.SaveChanges();

        // Username is unique - randomize so multiple calls (e.g. in GetAll tests) don't collide
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
}
