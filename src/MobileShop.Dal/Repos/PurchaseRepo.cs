namespace MobileShop.Dal.Repos;

public class PurchaseRepo(AppDbContext context) : BaseRepo<Purchase>(context), IPurchaseRepo
{
    public Purchase? FindWithPhones(int id)
        => Table.Include(p => p.Phones)
                .Include(p => p.SellerNavigation)
                .FirstOrDefault(p => p.Id == id);

    public async Task<Purchase?> FindWithPhonesAsync(int id)
        => await Table.Include(p => p.Phones)
                      .Include(p => p.SellerNavigation)
                      .FirstOrDefaultAsync(p => p.Id == id);
}
