namespace MobileShop.Tests.Dal.Repos;

public class SecondHandRepoTests : BaseRepoTests<SecondHand, ISecondHandRepo>
{
    protected override ISecondHandRepo CreateRepo() => new SecondHandRepo(Context);

    protected override SecondHand CreateValidEntity()
    {
        var product = new Product { Price = 100, Manufacturer = "TestCo", Model = "SH" };
        Context.Products.Add(product);
        Context.SaveChanges();

        return new SecondHand { TestPeriodDays = 7, UsedDurationDays = 30, ProductId = product.Id };
    }
}
