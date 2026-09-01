namespace MobileShop.Dal.Repos.Interfaces;

public interface IPhoneRepo : IBaseRepo<Phone>
{
    IEnumerable<Phone> GetInStock();
    bool ImeiExists(string imei1);

    Task<IEnumerable<Phone>> GetInStockAsync();
    Task<bool> ImeiExistsAsync(string imei1);
}
