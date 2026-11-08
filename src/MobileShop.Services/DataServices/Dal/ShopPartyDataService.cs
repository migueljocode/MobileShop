namespace MobileShop.Services.DataServices.Dal;

/// <summary>
/// Keeps the Zone Mobile shop party registered. The shop phone number comes from the application
/// settings (<see cref="PdfSettings.ShopPhone"/>), because a migration cannot read configuration.
/// </summary>
public class ShopPartyDataService(
    IBaseRepo<Person> people,
    IBaseRepo<Seller> sellers,
    IBaseRepo<Customer> customers,
    IOptions<PdfSettings> options,
    ILogger<ShopPartyDataService> logger)
    : IShopPartyDataService
{
    /// <inheritdoc />
    public async Task EnsureAsync()
    {
        var phone = options.Value.ShopPhone?.Trim();
        var person = await people.FindAsync(candidate =>
            candidate.FirstName + " " + candidate.LastName == ShopLabels.ZoneMobile);

        if (person is null)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                logger.LogWarning(
                    "The {Shop} shop party was not created because Pdf:ShopPhone is not configured.",
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
