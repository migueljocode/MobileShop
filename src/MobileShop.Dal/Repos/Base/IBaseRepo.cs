namespace MobileShop.Dal.Repos.Base;

/// <summary>
/// Generic CRUD repository contract shared by every entity repository.
/// Deletes are soft deletes - see <see cref="Delete"/>.
/// </summary>
/// <typeparam name="T">The entity type, which must derive from <see cref="BaseEntity"/>.</typeparam>
public interface IBaseRepo<T> where T : BaseEntity
{
    T? Find(int id);
    T? Find(Expression<Func<T, bool>> predicate);
    IEnumerable<T> FindAll(Expression<Func<T, bool>>? predicate = null);
    TResult? Select<TResult>(int id, Expression<Func<T, TResult>> selector);
    TResult? Select<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
    IEnumerable<TResult> SelectAll<TResult>(Expression<Func<T, TResult>> selector);
    IEnumerable<TResult> SelectAll<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);

    int Add(T entity, bool persist = true);
    int AddRange(IEnumerable<T> entities, bool persist = true);
    int Update(T entity, bool persist = true);
    int UpdateRange(IEnumerable<T> entities, bool persist = true);
    int Delete(T entity, bool persist = true);
    int DeleteRange(IEnumerable<T> entities, bool persist = true);
    int DeleteRange(Expression<Func<T, bool>> predicate, bool persist = true);
    int SaveChanges();

    Task<T?> FindAsync(int id);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? predicate = null);
    Task<TResult?> SelectAsync<TResult>(int id, Expression<Func<T, TResult>> selector);
    Task<TResult?> SelectAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
    Task<IEnumerable<TResult>> SelectAllAsync<TResult>(Expression<Func<T, TResult>> selector);
    Task<IEnumerable<TResult>> SelectAllAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);

    Task<int> AddAsync(T entity, bool persist = true);
    Task<int> AddRangeAsync(IEnumerable<T> entities, bool persist = true);
    Task<int> UpdateAsync(T entity, bool persist = true);
    Task<int> UpdateRangeAsync(IEnumerable<T> entities, bool persist = true);
    Task<int> DeleteAsync(T entity, bool persist = true);
    Task<int> DeleteRangeAsync(IEnumerable<T> entities, bool persist = true);
    Task<int> DeleteRangeAsync(Expression<Func<T, bool>> predicate, bool persist = true);
    Task<int> SaveChangesAsync();

    bool Any(Expression<Func<T, bool>>? predicate = null);
    Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null);
    int Count(Expression<Func<T, bool>>? predicate = null);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null);
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null);
}