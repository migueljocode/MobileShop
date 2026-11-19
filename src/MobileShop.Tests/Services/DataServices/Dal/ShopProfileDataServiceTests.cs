namespace MobileShop.Tests.Services.DataServices.Dal;

[Collection("MobileShop-Dal-Serial")]
public class ShopProfileDataServiceTests : RepoTestBase
{
    [Fact]
    public async Task EnsureAsync_seeds_from_existing_shop_party_phone_when_present()
    {
        Context.People.Add(new Person
        {
            FirstName = "Zone",
            LastName = "Mobile",
            PhoneNumber = "09121112233"
        });
        Context.SaveChanges();

        var settings = new PdfSettings
        {
            ShopName = "Zone Mobile",
            ShopAddress = "Main Street",
            ShopPhone = "+98 911 500 9109",
            ShopInstagram = "@zonemobile"
        };

        await CreateService(settings).EnsureAsync();

        var profile = Assert.Single(Context.ShopProfiles);
        Assert.Equal("Zone Mobile", profile.Name);
        Assert.Equal("Main Street", profile.Address);
        Assert.Equal("09121112233", profile.Phone);
        Assert.Equal("@zonemobile", profile.Instagram);
        Assert.Equal("09121112233", settings.ShopPhone);
    }

    [Fact]
    public async Task EnsureAsync_seeds_from_settings_when_no_shop_party_exists()
    {
        var settings = new PdfSettings
        {
            ShopName = "Mobile Shop",
            ShopAddress = "Gonbad",
            ShopPhone = "+98 911 500 9109",
            ShopInstagram = "@shop"
        };

        await CreateService(settings).EnsureAsync();

        var profile = Assert.Single(Context.ShopProfiles);
        Assert.Equal("Mobile Shop", profile.Name);
        Assert.Equal("Gonbad", profile.Address);
        Assert.Equal("+98 911 500 9109", profile.Phone);
        Assert.Equal("@shop", profile.Instagram);
    }

    [Fact]
    public async Task EnsureAsync_never_overwrites_an_existing_profile_from_settings_and_pushes_db_values_to_settings()
    {
        Context.ShopProfiles.Add(new ShopProfile
        {
            Name = "Saved Shop",
            Address = "Saved Address",
            Phone = "09129998877",
            Instagram = "@saved"
        });
        Context.SaveChanges();

        var settings = new PdfSettings
        {
            ShopName = "Config Name",
            ShopAddress = "Config Address",
            ShopPhone = "09000000000",
            ShopInstagram = "@config"
        };

        await CreateService(settings).EnsureAsync();

        var profile = Assert.Single(Context.ShopProfiles);
        Assert.Equal("Saved Shop", profile.Name);
        Assert.Equal("09129998877", profile.Phone);
        Assert.Equal("Saved Shop", settings.ShopName);
        Assert.Equal("Saved Address", settings.ShopAddress);
        Assert.Equal("09129998877", settings.ShopPhone);
        Assert.Equal("@saved", settings.ShopInstagram);
    }

    [Fact]
    public async Task SaveAsync_updates_row_shop_party_phone_and_pdf_settings()
    {
        Context.ShopProfiles.Add(new ShopProfile
        {
            Name = "Old Shop",
            Address = "Old Address",
            Phone = "09120000001",
            Instagram = "@old"
        });
        Context.SaveChanges();

        var settings = new PdfSettings();
        var result = await CreateService(settings).SaveAsync(new ShopProfileInput
        {
            Name = "  New Shop  ",
            Address = "",
            Phone = "  09123334455  ",
            Instagram = ""
        });

        Assert.True(result.Succeeded);
        var profile = Assert.Single(Context.ShopProfiles);
        Assert.Equal("New Shop", profile.Name);
        Assert.Equal(string.Empty, profile.Address);
        Assert.Equal("09123334455", profile.Phone);
        Assert.Equal(string.Empty, profile.Instagram);

        Assert.Equal("New Shop", settings.ShopName);
        Assert.Equal(string.Empty, settings.ShopAddress);
        Assert.Equal("09123334455", settings.ShopPhone);
        Assert.Equal(string.Empty, settings.ShopInstagram);

        var partyPerson = Assert.Single(Context.People);
        Assert.Equal("Zone", partyPerson.FirstName);
        Assert.Equal("Mobile", partyPerson.LastName);
        Assert.Equal("09123334455", partyPerson.PhoneNumber);
    }

    [Theory]
    [InlineData("", "09123334455", nameof(ShopProfileInput.Name))]
    [InlineData("   ", "09123334455", nameof(ShopProfileInput.Name))]
    [InlineData("Zone Mobile", "", nameof(ShopProfileInput.Phone))]
    [InlineData("Zone Mobile", "   ", nameof(ShopProfileInput.Phone))]
    public async Task SaveAsync_rejects_blank_name_or_phone(string name, string phone, string expectedField)
    {
        Context.ShopProfiles.Add(new ShopProfile
        {
            Name = "Existing Shop",
            Address = "Address",
            Phone = "09120000001",
            Instagram = "@existing"
        });
        Context.SaveChanges();

        var result = await CreateService(new PdfSettings()).SaveAsync(new ShopProfileInput
        {
            Name = name,
            Address = "Address",
            Phone = phone,
            Instagram = "@existing"
        });

        Assert.False(result.Succeeded);
        Assert.Equal(expectedField, result.ErrorField);
        Assert.Equal("Existing Shop", Assert.Single(Context.ShopProfiles).Name);
    }

    private ShopProfileDataService CreateService(PdfSettings settings)
    {
        var people = new BaseRepo<Person>(Context);
        var sellers = new BaseRepo<Seller>(Context);
        var customers = new BaseRepo<Customer>(Context);
        var profiles = new BaseRepo<ShopProfile>(Context);
        var partyService = new ShopPartyDataService(
            people,
            sellers,
            customers,
            profiles,
            NullLogger<ShopPartyDataService>.Instance);

        return new ShopProfileDataService(
            profiles,
            people,
            partyService,
            Options.Create(settings));
    }
}
