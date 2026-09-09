namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IPhoneRepo" />
public class PhoneRepo(AppDbContext context) : BaseRepo<Phone>(context), IPhoneRepo
{
    public Guarantee? GetGuarantee()
    {
        throw new NotImplementedException();
    }

    public Task<Guarantee?> GetGuaranteeAsync()
    {
        throw new NotImplementedException();
    }

    public Customer? GetOwner(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Customer?> GetOwnerAsync()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool ImeiExists(string imei1)
        => Table.Any(p => p.IMEI1 == imei1);

    /// <inheritdoc />
    public async Task<bool> ImeiExistsAsync(string imei1)
        => await Table.AnyAsync(p => p.IMEI1 == imei1);

    public bool IsSecondHand(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsSecondHandAsync(int id)
    {
        throw new NotImplementedException();
    }

    public bool IsSold(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsSoldAsync(int id)
    {
        throw new NotImplementedException();
    }

    public int Quantity()
    {
        throw new NotImplementedException();
    }

    public Task<int> QuantityAsync()
    {
        throw new NotImplementedException();
    }
}
