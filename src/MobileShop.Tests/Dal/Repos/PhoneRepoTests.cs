namespace MobileShop.Tests.Dal.Repos;

public class PhoneRepoTests : BaseRepoTests<Phone, IPhoneRepo>
{
    protected override IPhoneRepo CreateRepo() => new PhoneRepo(Context);

    protected override Phone CreateValidEntity()
    {
        var product = TestDataHelpers.CreateProduct(Context);

        return new Phone { IMEI1 = TestDataHelpers.GenerateImei(), ProductId = product.Id };
    }

}