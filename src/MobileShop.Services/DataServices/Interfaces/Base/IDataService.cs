namespace MobileShop.Services.DataServices.Interfaces.Base;

/// <summary>
/// Defines the public contract for IDataService.
/// </summary>
public interface IDataService<T> where T : BaseEntity
{
    IEnumerable<T> FindAll(Expression<Func<T, bool>>? predicate = null);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? predicate = null);

    IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null);
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null);

    T? Find(int id);
    T? Find(Expression<Func<T, bool>> predicate);
    Task<T?> FindAsync(int id);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);

    bool Add(T entity);
    bool Update(T entity);
    bool Delete(int id);
    bool Delete(T entity);

    Task<bool> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAsync(T entity);
}
