namespace MobileShop.Dal.Repos;

public class PhoneRepo(AppDbContext context) : BaseRepo<Phone>(context), IPhoneRepo
{
    public IEnumerable<Phone> GetInStock()
        => Table.Where(p => p.Status == PhoneStatus.InStock).ToList();

    public bool ImeiExists(string imei1)
        => Table.Any(p => p.IMEI1 == imei1);

    public async Task<IEnumerable<Phone>> GetInStockAsync()
        => await Table.Where(p => p.Status == PhoneStatus.InStock).ToListAsync();

    public async Task<bool> ImeiExistsAsync(string imei1)
        => await Table.AnyAsync(p => p.IMEI1 == imei1);
}
