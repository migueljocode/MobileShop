namespace MobileShop.Tests.Dal.Repos;

public class GuaranteeRepoTests : BaseRepoTests<Guarantee, IGuaranteeRepo>
{
    protected override IGuaranteeRepo CreateRepo() => new GuaranteeRepo(Context);

    protected override Guarantee CreateValidEntity()
    {
        var product = new Product { Price = 100, Manufacturer = "TestCo", Model = "G" };
        Context.Products.Add(product);
        Context.SaveChanges();

        return new Guarantee
        {
            StartDate = DateTime.Today,
            ExpirationDate = DateTime.Today.AddYears(1),
            Corporation = "TestCorp",
            ProductId = product.Id
        };
    }
}
