namespace MobileShop.Tests.Dal.Repos;

public class SecondHandRepoTests : BaseRepoTests<SecondHand, ISecondHandRepo>
{
    protected override ISecondHandRepo CreateRepo() => new SecondHandRepo(Context);

    protected override SecondHand CreateValidEntity()
    {
        var product = TestDataHelpers.CreateProduct(Context);

        return new SecondHand { TestPeriodDays = 7, UsedDurationDays = 30, ProductId = product.Id };
    }
}
