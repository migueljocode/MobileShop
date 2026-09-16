namespace MobileShop.Services.DataServices.Interfaces.Base;

/// <summary>
/// Defines the public contract for IDataService.
/// </summary>
public interface IDataService<T> where T : BaseEntity
{
    /// <summary>Finds all entities matching an optional filter.</summary>
    /// <param name="predicate">An optional database-translatable filter.</param>
    /// <returns>The matching, non-deleted entities.</returns>
    IEnumerable<T> FindAll(Expression<Func<T, bool>>? predicate = null);
    /// <summary>Finds all entities matching an optional filter asynchronously.</summary>
    /// <param name="predicate">An optional database-translatable filter.</param>
    /// <returns>The matching, non-deleted entities.</returns>
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>Finds an entity by identifier.</summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The entity, or <see langword="null"/> when not found.</returns>
    T? Find(int id);
    /// <summary>Finds the first entity matching a filter.</summary>
    /// <param name="predicate">The database-translatable filter.</param>
    /// <returns>The entity, or <see langword="null"/> when not found.</returns>
    T? Find(Expression<Func<T, bool>> predicate);
    /// <summary>Finds an entity by identifier asynchronously.</summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The entity, or <see langword="null"/> when not found.</returns>
    Task<T?> FindAsync(int id);
    /// <summary>Finds the first matching entity asynchronously.</summary>
    /// <param name="predicate">The database-translatable filter.</param>
    /// <returns>The entity, or <see langword="null"/> when not found.</returns>
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Adds an entity.</summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    bool Add(T entity);
    /// <summary>Updates an entity.</summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    bool Update(T entity);
    /// <summary>Soft-deletes an entity by identifier.</summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    bool Delete(int id);
    /// <summary>Soft-deletes an entity.</summary>
    /// <param name="entity">The entity to soft-delete.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    bool Delete(T entity);

    /// <summary>Adds an entity asynchronously.</summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    Task<bool> AddAsync(T entity);
    /// <summary>Updates an entity asynchronously.</summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    Task<bool> UpdateAsync(T entity);
    /// <summary>Soft-deletes an entity by identifier asynchronously.</summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    Task<bool> DeleteAsync(int id);
    /// <summary>Soft-deletes an entity asynchronously.</summary>
    /// <param name="entity">The entity to soft-delete.</param>
    /// <returns><see langword="true"/> when persistence succeeds.</returns>
    Task<bool> DeleteAsync(T entity);
}
