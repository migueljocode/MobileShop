using MobileShop.Services.DataServices.Shared;

namespace MobileShop.Tests.Services.DataServices.Shared;

[Collection("MobileShop-Dal-Serial")]
public class ShopPartyResolverTests : RepoTestBase
{
    private readonly BaseRepo<Person> _people;
    private readonly BaseRepo<Seller> _sellers;
    private readonly BaseRepo<Customer> _customers;

    public ShopPartyResolverTests()
    {
        _people = new BaseRepo<Person>(Context);
        _sellers = new BaseRepo<Seller>(Context);
        _customers = new BaseRepo<Customer>(Context);
    }

    [Fact]
    public async Task ResolveAsync_returns_actionable_error_when_the_shop_person_is_missing()
    {
        var result = await ShopPartyResolver.ResolveAsync(_people, _sellers, _customers);

        Assert.NotNull(result.Error);
        Assert.Contains("Zone Mobile", result.Error);
        Assert.Contains("People", result.Error);
        Assert.Equal(0, result.SellerId);
        Assert.Equal(0, result.CustomerId);
        Assert.Empty(Context.Sellers);
        Assert.Empty(Context.Customers);
    }

    [Fact]
    public async Task ResolveAsync_creates_the_seller_and_customer_when_only_the_person_exists()
    {
        AddShopPerson();

        var result = await ShopPartyResolver.ResolveAsync(_people, _sellers, _customers);

        Assert.Null(result.Error);
        var seller = Assert.Single(Context.Sellers);
        var customer = Assert.Single(Context.Customers);
        Assert.Equal(SellerEntityType.Legal, seller.EntityType);
        Assert.Equal("0099999999", customer.NationalId);
        Assert.Equal(seller.Id, result.SellerId);
        Assert.Equal(customer.Id, result.CustomerId);
    }

    [Fact]
    public async Task ResolveAsync_returns_the_existing_records_without_creating_duplicates()
    {
        var person = AddShopPerson();
        var seller = new Seller { PersonId = person.Id, EntityType = SellerEntityType.Legal };
        var customer = new Customer { PersonId = person.Id, NationalId = "0000000000" };
        Context.Sellers.Add(seller);
        Context.Customers.Add(customer);
        Context.SaveChanges();

        var first = await ShopPartyResolver.ResolveAsync(_people, _sellers, _customers);
        var second = await ShopPartyResolver.ResolveAsync(_people, _sellers, _customers);

        Assert.Null(first.Error);
        Assert.Equal(new ShopParty(seller.Id, customer.Id, null), first);
        Assert.Equal(first, second);
        Assert.Single(Context.Sellers);
        Assert.Single(Context.Customers);
    }

    [Fact]
    public async Task ResolveAsync_creates_only_the_customer_when_the_seller_already_exists()
    {
        var person = AddShopPerson();
        var seller = new Seller { PersonId = person.Id, EntityType = SellerEntityType.Legal };
        Context.Sellers.Add(seller);
        Context.SaveChanges();

        var result = await ShopPartyResolver.ResolveAsync(_people, _sellers, _customers);

        Assert.Null(result.Error);
        Assert.Equal(seller.Id, result.SellerId);
        var customer = Assert.Single(Context.Customers);
        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Single(Context.Sellers);
    }

    private Person AddShopPerson()
    {
        var person = new Person { FirstName = "Zone", LastName = "Mobile", PhoneNumber = "09120000000" };
        Context.People.Add(person);
        Context.SaveChanges();
        return person;
    }
}
