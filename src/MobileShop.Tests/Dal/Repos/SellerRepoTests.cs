namespace MobileShop.Tests.Dal.Repos;

public class SellerRepoTests : BaseRepoTests<Seller, ISellerRepo>
{
    protected override ISellerRepo CreateRepo() => new SellerRepo(Context);

    protected override Seller CreateValidEntity()
    {
        var person = new Person { FirstName = "Test", LastName = "Seller", PhoneNumber = "09120000000" };
        Context.People.Add(person);
        Context.SaveChanges();

        return new Seller { PersonId = person.Id, EntityType = SellerEntityType.Real };
    }
}
