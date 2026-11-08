using Microsoft.Extensions.Options;
using MobileShop.Services.PDF.Settings;

namespace MobileShop.Tests.Services.DataServices.Dal;

[Collection("MobileShop-Dal-Serial")]
public class ShopPartyDataServiceTests : RepoTestBase
{
    private const string ConfiguredPhone = "+98 912 000 0000";
    private const string OtherPhone = "09120000009";

    [Fact]
    public async Task EnsureAsync_creates_the_shop_party_with_the_configured_phone_when_missing()
    {
        await CreateService(ConfiguredPhone).EnsureAsync();

        var person = Assert.Single(Context.People);
        Assert.Equal("Zone", person.FirstName);
        Assert.Equal("Mobile", person.LastName);
        Assert.Equal(ConfiguredPhone, person.PhoneNumber);
        var seller = Assert.Single(Context.Sellers);
        Assert.Equal(person.Id, seller.PersonId);
        Assert.Equal(SellerEntityType.Legal, seller.EntityType);
        Assert.Equal(person.Id, Assert.Single(Context.Customers).PersonId);
    }

    [Fact]
    public async Task EnsureAsync_creates_nothing_when_the_party_is_missing_and_no_phone_is_configured()
    {
        await CreateService(string.Empty).EnsureAsync();

        Assert.Empty(Context.People);
        Assert.Empty(Context.Sellers);
        Assert.Empty(Context.Customers);
    }

    [Fact]
    public async Task EnsureAsync_updates_the_phone_of_an_existing_shop_person_to_the_configured_value()
    {
        AddShopPerson(OtherPhone);

        await CreateService(ConfiguredPhone).EnsureAsync();

        Assert.Equal(ConfiguredPhone, Assert.Single(Context.People).PhoneNumber);
        Assert.Single(Context.Sellers);
        Assert.Single(Context.Customers);
    }

    [Fact]
    public async Task EnsureAsync_keeps_the_phone_of_an_existing_shop_person_when_none_is_configured()
    {
        AddShopPerson(OtherPhone);

        await CreateService(string.Empty).EnsureAsync();

        Assert.Equal(OtherPhone, Assert.Single(Context.People).PhoneNumber);
        Assert.Single(Context.Sellers);
        Assert.Single(Context.Customers);
    }

    [Fact]
    public async Task EnsureAsync_is_idempotent()
    {
        var service = CreateService(ConfiguredPhone);

        await service.EnsureAsync();
        await service.EnsureAsync();

        Assert.Single(Context.People);
        Assert.Single(Context.Sellers);
        Assert.Single(Context.Customers);
    }

    private ShopPartyDataService CreateService(string shopPhone) => new(
        new BaseRepo<Person>(Context),
        new BaseRepo<Seller>(Context),
        new BaseRepo<Customer>(Context),
        Options.Create(new PdfSettings { ShopPhone = shopPhone }),
        NullLogger<ShopPartyDataService>.Instance);

    private void AddShopPerson(string phoneNumber)
    {
        Context.People.Add(new Person { FirstName = "Zone", LastName = "Mobile", PhoneNumber = phoneNumber });
        Context.SaveChanges();
    }
}
