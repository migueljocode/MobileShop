namespace MobileShop.Tests.Dal.Repos;

public class AppleIdRepoTests : BaseRepoTests<AppleId, IAppleIdRepo>
{
    protected override IAppleIdRepo CreateRepo() => new AppleIdRepo(Context);

    protected override AppleId CreateValidEntity()
    {
        var product = new Product { Price = 100, Manufacturer = "Apple", Model = "A" };
        Context.Products.Add(product);
        Context.SaveChanges();

        return new AppleId { Email = $"{Guid.NewGuid():N}@example.com", Password = "pw", ProductId = product.Id };
    }
}
