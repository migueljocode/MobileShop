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

    [Fact]
    public void Find_EagerlyLoadsPersonNavigation()
    {
        var repo = CreateRepo();
        var customer = CreateValidEntity();
        repo.Add(customer);

        var found = repo.Find(customer.Id);

        Assert.NotNull(found);
        Assert.NotNull(found!.PersonNavigation);
        Assert.Equal("Customer", found.PersonNavigation.LastName);
    }

    [Fact]
    public async Task FindAsync_EagerlyLoadsPersonNavigation()
    {
        var repo = CreateRepo();
        var customer = CreateValidEntity();
        await repo.AddAsync(customer);

        var found = await repo.FindAsync(customer.Id);

        Assert.NotNull(found);
        Assert.NotNull(found!.PersonNavigation);
        Assert.Equal("Customer", found.PersonNavigation.LastName);
    }
}
