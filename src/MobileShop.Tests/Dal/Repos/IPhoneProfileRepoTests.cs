namespace MobileShop.Tests.Dal.Repos;

public class IPhoneProfileRepoTests : BaseRepoTests<IPhone, IIPhoneProfileRepo>
{
    protected override IIPhoneProfileRepo CreateRepo() => new IPhoneProfileRepo(Context);

    protected override IPhone CreateValidEntity()
    {
        var product = new Product { Price = 100, Manufacturer = "Apple", Model = "IP" };
        Context.Products.Add(product);
        Context.SaveChanges();

        var phone = new Phone { IMEI1 = TestDataHelpers.GenerateImei(), ProductId = product.Id };
        Context.Phones.Add(phone);
        Context.SaveChanges();

        return new IPhone { PhoneId = phone.Id, BatteryHealth = 90 };
    }
}
