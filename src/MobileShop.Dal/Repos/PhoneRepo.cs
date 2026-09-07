namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IPhoneRepo" />
public class PhoneRepo(AppDbContext context) : BaseRepo<Phone>(context), IPhoneRepo
{
    /// <inheritdoc />
    public bool ImeiExists(string imei1)
        => Table.Any(p => p.IMEI1 == imei1);

    /// <inheritdoc />
    public async Task<bool> ImeiExistsAsync(string imei1)
        => await Table.AnyAsync(p => p.IMEI1 == imei1);
}
