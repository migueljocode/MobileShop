namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IAppleIdRepo" />
public class AppleIdRepo(AppDbContext context) : BaseRepo<AppleId>(context), IAppleIdRepo
{
    public AppleId? Find(string email)
        => Table.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    

    public async Task<AppleId?> FindAsync(string email)
        => await Table.FirstOrDefaultAsync(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    
    public Customer? FindUser(int id)
    {
        var entity = Table.Include(x => x.ProductNavigation)
        .ThenInclude(y => y.Transactions)
        .ThenInclude(z => z.CustomerNavigation)
        .FirstOrDefault(x => x.Id == id);
        if (entity is null)
            return null;
        return entity
        .ProductNavigation
        .Transactions
        .Select(a => a.CustomerNavigation)
        .FirstOrDefault();
    }

    public Task<Customer?> FindUserAsync(int id)
    {
        throw new NotImplementedException();
    }

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
