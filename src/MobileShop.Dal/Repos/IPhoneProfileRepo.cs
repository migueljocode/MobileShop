namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IIPhoneProfileRepo" />
public class IPhoneProfileRepo(AppDbContext context) : BaseRepo<IPhone>(context), IIPhoneProfileRepo
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
