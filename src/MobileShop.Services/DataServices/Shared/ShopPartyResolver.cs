namespace MobileShop.Services.DataServices.Shared;

/// <summary>The shop's own seller and customer records, resolved by <see cref="ShopPartyResolver"/>.</summary>
/// <param name="SellerId">The shop's seller id; 0 when resolution failed.</param>
/// <param name="CustomerId">The shop's customer id; 0 when resolution failed.</param>
/// <param name="Error">The failure message, or null when resolution succeeded.</param>
public sealed record ShopParty(int SellerId, int CustomerId, string? Error);

/// <summary>
/// Resolves the shop (<see cref="ShopLabels.ZoneMobile"/>) as the seller of every sale and the
/// customer of every purchase. The shop is the Person whose full name equals the label; its seller
/// and customer roles are created on demand.
/// </summary>
public static class ShopPartyResolver
{
    private const string PlaceholderNationalId = "0099999999";

    /// <summary>Finds the shop person and gets or creates its seller and customer records.</summary>
    public static async Task<ShopParty> ResolveAsync(
        IBaseRepo<Person> people,
        IBaseRepo<Seller> sellers,
        IBaseRepo<Customer> customers)
    {
        var person = await people.FindAsync(candidate =>
            candidate.FirstName + " " + candidate.LastName == ShopLabels.ZoneMobile);
        if (person is null)
            return Failure($"{ShopLabels.ZoneMobile} (the shop) must be registered before this can be recorded. " +
                           "In People > Customers, add a customer with first name Zone and last name Mobile, then try again.");

        var seller = await sellers.FindAsync(candidate => candidate.PersonId == person.Id);
        if (seller is null)
        {
            seller = new Seller { PersonId = person.Id, EntityType = SellerEntityType.Legal };
            if (await sellers.AddAsync(seller) <= 0)
                return Failure($"{ShopLabels.ZoneMobile} could not be registered as a seller.");
        }

        var customer = await customers.FindAsync(candidate => candidate.PersonId == person.Id);
        if (customer is null)
        {
            customer = new Customer { PersonId = person.Id, NationalId = PlaceholderNationalId };
            if (await customers.AddAsync(customer) <= 0)
                return Failure($"{ShopLabels.ZoneMobile} could not be registered as a customer.");
        }

        return new ShopParty(seller.Id, customer.Id, null);
    }

    private static ShopParty Failure(string error) => new(0, 0, error);
}
