namespace MobileShop.Tests.Services.DataServices.Dal;

[Collection("MobileShop-Dal-Serial")]
public class ShopPartyDataServiceTests : RepoTestBase
{
    private const string ProfilePhone = "+98 912 000 0000";
    private const string OtherPhone = "09120000009";

    [Fact]
    public async Task EnsureAsync_creates_the_shop_party_with_the_profile_phone_when_missing()
    {
        AddShopProfile(ProfilePhone);

        await CreateService().EnsureAsync();

        var person = Assert.Single(Context.People);
        Assert.Equal("Zone", person.FirstName);
        Assert.Equal("Mobile", person.LastName);
        Assert.Equal(ProfilePhone, person.PhoneNumber);
        var seller = Assert.Single(Context.Sellers);
        Assert.Equal(person.Id, seller.PersonId);
        Assert.Equal(SellerEntityType.Legal, seller.EntityType);
        Assert.Equal(person.Id, Assert.Single(Context.Customers).PersonId);
    }

    [Fact]
    public async Task EnsureAsync_does_nothing_when_no_shop_profile_row_exists()
    {
        await CreateService().EnsureAsync();

        Assert.Empty(Context.People);
        Assert.Empty(Context.Sellers);
        Assert.Empty(Context.Customers);
    }

    [Fact]
    public async Task EnsureAsync_creates_nothing_when_the_party_is_missing_and_profile_phone_is_blank()
    {
        AddShopProfile(string.Empty);

        await CreateService().EnsureAsync();

        Assert.Empty(Context.People);
        Assert.Empty(Context.Sellers);
        Assert.Empty(Context.Customers);
    }

    [Fact]
    public async Task EnsureAsync_updates_the_phone_of_an_existing_shop_person_to_the_profile_value()
    {
        AddShopProfile(ProfilePhone);
        AddShopPerson(OtherPhone);

        await CreateService().EnsureAsync();

        Assert.Equal(ProfilePhone, Assert.Single(Context.People).PhoneNumber);
        Assert.Single(Context.Sellers);
        Assert.Single(Context.Customers);
    }

    [Fact]
    public async Task EnsureAsync_keeps_the_phone_of_an_existing_shop_person_when_profile_phone_is_blank()
    {
        AddShopProfile(string.Empty);
        AddShopPerson(OtherPhone);

        await CreateService().EnsureAsync();

        Assert.Equal(OtherPhone, Assert.Single(Context.People).PhoneNumber);
        Assert.Single(Context.Sellers);
        Assert.Single(Context.Customers);
    }

    [Fact]
    public async Task EnsureAsync_is_idempotent()
    {
        AddShopProfile(ProfilePhone);
        var service = CreateService();

        await service.EnsureAsync();
        await service.EnsureAsync();

        Assert.Single(Context.People);
        Assert.Single(Context.Sellers);
        Assert.Single(Context.Customers);
    }

    private ShopPartyDataService CreateService() => new(
        new BaseRepo<Person>(Context),
        new BaseRepo<Seller>(Context),
        new BaseRepo<Customer>(Context),
        new BaseRepo<ShopProfile>(Context),
        NullLogger<ShopPartyDataService>.Instance);

    private void AddShopProfile(string phone)
    {
        Context.ShopProfiles.Add(new ShopProfile
        {
            Name = "Zone Mobile",
            Address = "Test Address",
            Phone = phone,
            Instagram = "@zonemobile"
        });
        Context.SaveChanges();
    }

    private void AddShopPerson(string phoneNumber)
    {
        Context.People.Add(new Person { FirstName = "Zone", LastName = "Mobile", PhoneNumber = phoneNumber });
        Context.SaveChanges();
    }
}
