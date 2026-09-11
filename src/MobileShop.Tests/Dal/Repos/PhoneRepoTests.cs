namespace MobileShop.Tests.Dal.Repos;

public class PhoneRepoTests : ForSaleRepoTests<Phone, IPhoneRepo>
{
    protected override IPhoneRepo CreateRepo() => new PhoneRepo(Context);

    protected override int GetProductId(Phone entity) => entity.ProductId;

    protected override Phone CreateValidEntity()
    {
        var product = new Product { Price = 100, Manufacturer = "TestCo", Model = "P" };
        Context.Products.Add(product);
        Context.SaveChanges();

        return new Phone { IMEI1 = TestDataHelpers.GenerateImei(), ProductId = product.Id };
    }

    [Fact]
    public void ImeiExists_ReturnsTrue_WhenImeiIsOnRecord()
    {
        var repo = CreateRepo();
        var phone = CreateValidEntity();
        repo.Add(phone);

        Assert.True(repo.ImeiExists(phone.IMEI1));
    }

    [Fact]
    public void ImeiExists_ReturnsFalse_WhenImeiIsUnknown()
        => Assert.False(CreateRepo().ImeiExists(TestDataHelpers.GenerateImei()));

    [Fact]
    public async Task ImeiExistsAsync_ReturnsTrue_WhenImeiIsOnRecord()
    {
        var repo = CreateRepo();
        var phone = CreateValidEntity();
        await repo.AddAsync(phone);

        Assert.True(await repo.ImeiExistsAsync(phone.IMEI1));
    }

    [Fact]
    public async Task ImeiExistsAsync_ReturnsFalse_WhenImeiIsUnknown()
        => Assert.False(await CreateRepo().ImeiExistsAsync(TestDataHelpers.GenerateImei()));
}