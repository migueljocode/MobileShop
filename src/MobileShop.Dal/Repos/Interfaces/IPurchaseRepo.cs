namespace MobileShop.Dal.Repos.Interfaces;

public interface IPurchaseRepo : IBaseRepo<Purchase>
{
    Purchase? FindWithPhones(int id);
    Task<Purchase?> FindWithPhonesAsync(int id);
}
