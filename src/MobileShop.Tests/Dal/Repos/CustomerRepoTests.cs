namespace MobileShop.Tests.Dal.Repos;

public class CustomerRepoTests : BaseRepoTests<Customer, ICustomerRepo>
{
    protected override ICustomerRepo CreateRepo() => new CustomerRepo(Context);

    protected override Customer CreateValidEntity()
    {
        var person = new Person { FirstName = "Test", LastName = "Customer", PhoneNumber = "09120000000" };
        Context.People.Add(person);
        Context.SaveChanges();

        return new Customer { PersonId = person.Id, NationalId = "1234567890" };
    }
}
