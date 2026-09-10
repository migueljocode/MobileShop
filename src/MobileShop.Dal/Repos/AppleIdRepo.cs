namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IAppleIdRepo" />
public class AppleIdRepo(AppDbContext context) : BaseRepo<AppleId>(context), IAppleIdRepo
{
    public AppleId? Find(string email)
        => Table.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    

    public async Task<AppleId?> FindAsync(string email)
        => await Table.FirstOrDefaultAsync(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    

    public Guarantee? GetGuarantee(int id)
        => Table
            .Include(x => x.ProductNavigation)
                .ThenInclude(x => x.GuaranteeProfile)
            .FirstOrDefault(x => x.Id == id)?
            .ProductNavigation?
            .GuaranteeProfile;
    

    public async Task<Guarantee?> GetGuaranteeAsync(int id)
    {
        var appleId = await Table
            .Include(x => x.ProductNavigation)
                .ThenInclude(x => x.GuaranteeProfile)
            .FirstOrDefaultAsync(x => x.Id == id);

        return appleId?
            .ProductNavigation?
            .GuaranteeProfile;
    }

    public Customer? GetOwner(int id)
    {        
        var appleId = 
        Table.Include(a => a.ProductNavigation)
                .ThenInclude(p => p.Transactions)
                    .ThenInclude(t => t.CustomerNavigation)
                        .ThenInclude(c => c.PersonNavigation)
            .FirstOrDefault(a => a.Id == id);

        return appleId?
                .ProductNavigation?
                .Transactions?
                .OrderByDescending(x => x.Date)
                .FirstOrDefault(x => x.Direction == TransactionDirection.Sell)?
                .CustomerNavigation;
    }

    public async Task<Customer?> GetOwnerAsync(int id)
    {
        var appleId = await Table
            .Include(a => a.ProductNavigation)
                .ThenInclude(p => p.Transactions)
                    .ThenInclude(t => t.CustomerNavigation)
            .FirstOrDefaultAsync(a => a.Id == id);

        return appleId?
            .ProductNavigation?
            .Transactions?
            .OrderByDescending(t => t.Date)
            .FirstOrDefault(t => t.Direction == TransactionDirection.Sell)?
            .CustomerNavigation;
    }

    public bool IsSecondHand(int id)
        => Table
            .Where(x => x.Id == id)
            .Select(x => x.ProductNavigation.SecondHandProfile)
            .Any(profile => profile != null);

    public async Task<bool> IsSecondHandAsync(int id)
        => await Table
            .Where(x => x.Id == id)
            .Select(x => x.ProductNavigation.SecondHandProfile)
            .AnyAsync(profile => profile != null);

    public bool IsSold(int id)
        => Table
            .Where(x => x.Id == id)
            .SelectMany(x => x.ProductNavigation.Transactions)
            .Any(t => t.Direction == TransactionDirection.Sell);

    public async Task<bool> IsSoldAsync(int id)
        => await Table
            .Where(x => x.Id == id)
            .SelectMany(x => x.ProductNavigation.Transactions)
            .AnyAsync(t => t.Direction == TransactionDirection.Sell);

    public int Quantity()
        => Table.Count(a => !a.ProductNavigation.Transactions
            .Any(t => t.Direction == TransactionDirection.Sell));

    public async Task<int> QuantityAsync()
        => await Table.CountAsync(a => !a.ProductNavigation.Transactions
            .Any(t => t.Direction == TransactionDirection.Sell));
}
