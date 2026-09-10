namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IPhoneRepo" />
public class PhoneRepo(AppDbContext context) : BaseRepo<Phone>(context), IPhoneRepo
{
    public Guarantee? GetGuarantee(int id)
        => Table
            .Where(p => p.Id == id)
            .Select(p => p.ProductNavigation.GuaranteeProfile)
            .FirstOrDefault();

    public async Task<Guarantee?> GetGuaranteeAsync(int id)
        => await Table
            .Where(p => p.Id == id)
            .Select(p => p.ProductNavigation.GuaranteeProfile)
            .FirstOrDefaultAsync();

    public Customer? GetOwner(int id)
        => Table
            .Where(p => p.Id == id)
            .SelectMany(p => p.ProductNavigation.Transactions)
            .Where(t => t.Direction == TransactionDirection.Sell)
            .OrderByDescending(t => t.Date)
            .Select(t => t.CustomerNavigation)
            .FirstOrDefault();

    public async Task<Customer?> GetOwnerAsync(int id)
        => await Table
            .Where(p => p.Id == id)
            .SelectMany(p => p.ProductNavigation.Transactions)
            .Where(t => t.Direction == TransactionDirection.Sell)
            .OrderByDescending(t => t.Date)
            .Select(t => t.CustomerNavigation)
            .FirstOrDefaultAsync();
            
    /// <inheritdoc />
    public bool ImeiExists(string imei1)
        => Table.Any(p => p.IMEI1 == imei1);

    /// <inheritdoc />
    public async Task<bool> ImeiExistsAsync(string imei1)
        => await Table.AnyAsync(p => p.IMEI1 == imei1);

    public bool IsSecondHand(int id)
        => Table
            .Where(p => p.Id == id)
            .Select(p => p.ProductNavigation.SecondHandProfile)
            .Any(profile => profile != null);

    public async Task<bool> IsSecondHandAsync(int id)
        => await Table
            .Where(p => p.Id == id)
            .Select(p => p.ProductNavigation.SecondHandProfile)
            .AnyAsync(profile => profile != null);

    public bool IsSold(int id)
        => Table
            .Where(p => p.Id == id)
            .SelectMany(p => p.ProductNavigation.Transactions)
            .Any(t => t.Direction == TransactionDirection.Sell);

    public async Task<bool> IsSoldAsync(int id)
        => await Table
            .Where(p => p.Id == id)
            .SelectMany(p => p.ProductNavigation.Transactions)
            .AnyAsync(t => t.Direction == TransactionDirection.Sell);

    public int Quantity()
        => Table.Count(p => !p.ProductNavigation.Transactions
            .Any(t => t.Direction == TransactionDirection.Sell));

    public async Task<int> QuantityAsync()
        => await Table.CountAsync(p => !p.ProductNavigation.Transactions
            .Any(t => t.Direction == TransactionDirection.Sell));
}
