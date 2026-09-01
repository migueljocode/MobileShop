namespace MobileShop.Dal.Repos.Base;

public interface IBaseRepo<T> where T : BaseEntity
{
    T? Find(int id);
    IEnumerable<T> GetAll();
    int Add(T entity, bool persist = true);
    int Update(T entity, bool persist = true);
    int Delete(T entity, bool persist = true);   // soft delete - IsDeleted رو ست می‌کنه
    int SaveChanges();

    Task<T?> FindAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<int> AddAsync(T entity, bool persist = true);
    Task<int> UpdateAsync(T entity, bool persist = true);
    Task<int> DeleteAsync(T entity, bool persist = true);   // soft delete
    Task<int> SaveChangesAsync();
}
