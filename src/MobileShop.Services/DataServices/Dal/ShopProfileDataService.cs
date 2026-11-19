namespace MobileShop.Services.DataServices.Dal;

/// <summary>
/// Manages the single-row <see cref="ShopProfile"/> in the database and keeps
/// <see cref="PdfSettings"/> and the Zone Mobile shop party synchronized with it.
/// </summary>
public class ShopProfileDataService(
    IBaseRepo<ShopProfile> profiles,
    IBaseRepo<Person> people,
    IShopPartyDataService shopPartyDataService,
    IOptions<PdfSettings> options)
    : IShopProfileDataService
{
    /// <inheritdoc />
    public async Task EnsureAsync()
    {
        var profile = await profiles.FindAsync(_ => true);
        if (profile is null)
        {
            var shopPerson = await people.FindAsync(candidate =>
                candidate.FirstName + " " + candidate.LastName == ShopLabels.ZoneMobile);
            var settings = options.Value;
            var phone = !string.IsNullOrWhiteSpace(shopPerson?.PhoneNumber)
                ? shopPerson.PhoneNumber.Trim()
                : (settings.ShopPhone ?? string.Empty).Trim();

            profile = new ShopProfile
            {
                Name = string.IsNullOrWhiteSpace(settings.ShopName) ? "Mobile Shop" : settings.ShopName.Trim(),
                Address = (settings.ShopAddress ?? string.Empty).Trim(),
                Phone = phone,
                Instagram = (settings.ShopInstagram ?? string.Empty).Trim()
            };

            await profiles.AddAsync(profile);
        }

        ApplyToPdfSettings(profile);
    }

    /// <inheritdoc />
    public async Task<ShopProfileView> GetAsync()
    {
        var profile = await profiles.FindAsync(_ => true);
        if (profile is null)
        {
            await EnsureAsync();
            profile = await profiles.FindAsync(_ => true);
        }

        return profile is null
            ? new ShopProfileView(
                options.Value.ShopName,
                options.Value.ShopAddress,
                options.Value.ShopPhone,
                options.Value.ShopInstagram)
            : new ShopProfileView(profile.Name, profile.Address, profile.Phone, profile.Instagram);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SaveAsync(ShopProfileInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var name = input.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            return new ServiceResult(false, "Shop name is required.", nameof(ShopProfileInput.Name));
        if (name.Length > 100)
            return new ServiceResult(false, "Shop name cannot exceed 100 characters.", nameof(ShopProfileInput.Name));

        var address = input.Address?.Trim() ?? string.Empty;
        if (address.Length > 300)
            return new ServiceResult(false, "Address cannot exceed 300 characters.", nameof(ShopProfileInput.Address));

        var phone = input.Phone?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(phone))
            return new ServiceResult(false, "Phone is required.", nameof(ShopProfileInput.Phone));
        if (phone.Length > 30)
            return new ServiceResult(false, "Phone cannot exceed 30 characters.", nameof(ShopProfileInput.Phone));

        var instagram = input.Instagram?.Trim() ?? string.Empty;
        if (instagram.Length > 100)
            return new ServiceResult(false, "Instagram cannot exceed 100 characters.", nameof(ShopProfileInput.Instagram));

        var profile = await profiles.FindAsync(_ => true);
        if (profile is null)
        {
            profile = new ShopProfile
            {
                Name = name,
                Address = address,
                Phone = phone,
                Instagram = instagram
            };
            if (await profiles.AddAsync(profile) <= 0)
                return new ServiceResult(false, "The shop profile could not be saved.");
        }
        else
        {
            profile.Name = name;
            profile.Address = address;
            profile.Phone = phone;
            profile.Instagram = instagram;
            if (await profiles.UpdateAsync(profile) <= 0)
                return new ServiceResult(false, "The shop profile could not be saved.");
        }

        ApplyToPdfSettings(profile);
        await shopPartyDataService.EnsureAsync();
        return new ServiceResult(true, "Shop profile updated successfully.");
    }

    private void ApplyToPdfSettings(ShopProfile profile)
    {
        // Deliberate in-place update of the singleton IOptions<PdfSettings>.Value instance:
        // QuestPdfGenerator holds IOptions<PdfSettings> and reads its properties on each render,
        // so updating these values here switches invoices to database values without changing the generator.
        var settings = options.Value;
        settings.ShopName = profile.Name;
        settings.ShopAddress = profile.Address;
        settings.ShopPhone = profile.Phone;
        settings.ShopInstagram = profile.Instagram;
    }
}
