namespace MobileShop.Tests.Dal.Repos;

public class AppleIdRepoTests : BaseRepoTests<AppleId, IAppleIdRepo>
{
    protected override IAppleIdRepo CreateRepo() => new AppleIdRepo(Context);

    protected override AppleId CreateValidEntity()
    {
        var product = TestDataHelpers.CreateProduct(Context);

        return new AppleId { Email = $"{Guid.NewGuid():N}@example.com", Password = "pw", ProductId = product.Id };
    }

    [Fact]
    public void Find_Email_ReturnsMatch_CaseInsensitively()
    {
        var repo = CreateRepo();
        var appleId = CreateValidEntity();
        repo.Add(appleId);

        var found = repo.Find(appleId.Email.ToUpperInvariant());

        Assert.NotNull(found);
        Assert.Equal(appleId.Id, found!.Id);
    }

    [Fact]
    public void Find_Email_ReturnsNull_WhenNoMatch()
        => Assert.Null(CreateRepo().Find("does-not-exist@example.com"));

    [Fact]
    public async Task FindAsync_Email_ReturnsMatch_CaseInsensitively()
    {
        var repo = CreateRepo();
        var appleId = CreateValidEntity();
        await repo.AddAsync(appleId);

        var found = await repo.FindAsync(appleId.Email.ToUpperInvariant());

        Assert.NotNull(found);
        Assert.Equal(appleId.Id, found!.Id);
    }

    [Fact]
    public async Task FindAsync_Email_ReturnsNull_WhenNoMatch()
        => Assert.Null(await CreateRepo().FindAsync("does-not-exist@example.com"));
}