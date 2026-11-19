namespace MobileShop.Services.DataServices.Dal;

/// <summary>
/// Keeps the Zone Mobile shop party registered using the phone number stored in the database
/// <see cref="ShopProfile"/> row.
/// </summary>
public class ShopPartyDataService(
    IBaseRepo<Person> people,
    IBaseRepo<Seller> sellers,
    IBaseRepo<Customer> customers,
    IBaseRepo<ShopProfile> shopProfiles,
    ILogger<ShopPartyDataService> logger)
    : IShopPartyDataService
{
    /// <inheritdoc />
    public async Task EnsureAsync()
    {
        var profile = await shopProfiles.FindAsync(_ => true);
        if (profile is null)
        {
            logger.LogWarning(
                "The {Shop} shop party was not ensured because no shop profile row exists yet.",
                ShopLabels.ZoneMobile);
            return;
        }

        var phone = profile.Phone?.Trim();
        var person = await people.FindAsync(candidate =>
            candidate.FirstName + " " + candidate.LastName == ShopLabels.ZoneMobile);
        if (person is null)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                logger.LogWarning(
                    "The {Shop} shop party was not created because the shop profile has no phone number.",
                    ShopLabels.ZoneMobile);
                return;
            }

            person = new Person { FirstName = "Zone", LastName = "Mobile", PhoneNumber = phone, Notes = "The shop" };
            if (await people.AddAsync(person) <= 0)
            {
                logger.LogWarning("The {Shop} shop person could not be created.", ShopLabels.ZoneMobile);
                return;
            }
        }
        else if (!string.IsNullOrWhiteSpace(phone) && person.PhoneNumber != phone)
        {
            person.PhoneNumber = phone;
            await people.UpdateAsync(person);
        }

        var shop = await ShopPartyResolver.ResolveAsync(people, sellers, customers);
        if (shop.Error is not null)
            logger.LogWarning("{Error}", shop.Error);
    }
}
