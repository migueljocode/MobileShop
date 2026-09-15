namespace MobileShop.Tests.Dal.Repos;

public class PhoneRepoTests : BaseRepoTests<Phone, IPhoneRepo>
{
    protected override IPhoneRepo CreateRepo() => new PhoneRepo(Context);

    protected override Phone CreateValidEntity()
    {
        var product = new Product { Price = 100, Manufacturer = "TestCo", Model = "P" };
        Context.Products.Add(product);
        Context.SaveChanges();

        return new Phone { IMEI1 = TestDataHelpers.GenerateImei(), ProductId = product.Id };
    }

}