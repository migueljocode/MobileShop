namespace MobileShop.Dal.Repos.Base;

/// <summary>
/// Generic CRUD repository contract shared by every entity repository.
/// Deletes are soft deletes - see <see cref="Delete"/>.
/// </summary>
/// <typeparam name="T">The entity type, which must derive from <see cref="BaseEntity"/>.</typeparam>
public interface IBaseRepo<T> where T : BaseEntity
{
    /// <summary>Finds an entity by its identifier, excluding soft-deleted entities.</summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The matching entity, or <see langword="null"/> when none exists.</returns>
    T? Find(int id);
    /// <summary>Finds the first entity matching a predicate.</summary>
    /// <param name="predicate">The database-translatable filter.</param>
    /// <returns>The first matching entity, or <see langword="null"/> when none exists.</returns>
    T? Find(Expression<Func<T, bool>> predicate);
    /// <summary>Finds all entities matching an optional predicate.</summary>
    /// <param name="predicate">The optional database-translatable filter.</param>
    /// <returns>The matching, non-deleted entities.</returns>
    IEnumerable<T> FindAll(Expression<Func<T, bool>>? predicate = null);
    /// <summary>Projects the entity with the specified identifier.</summary>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    /// <param name="id">The entity identifier.</param>
    /// <param name="selector">The database-translatable projection.</param>
    /// <returns>The projected result, or <see langword="null"/> when none exists.</returns>
    TResult? Select<TResult>(int id, Expression<Func<T, TResult>> selector);
    /// <summary>Projects the first entity matching a predicate.</summary>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    /// <param name="predicate">The database-translatable filter.</param>
    /// <param name="selector">The database-translatable projection.</param>
    /// <returns>The projected result, or <see langword="null"/> when none exists.</returns>
    TResult? Select<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
    /// <summary>Projects all entities.</summary>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    /// <param name="selector">The database-translatable projection.</param>
    /// <returns>The projected, non-deleted results.</returns>
    IEnumerable<TResult> SelectAll<TResult>(Expression<Func<T, TResult>> selector);
    /// <summary>Projects all entities matching a predicate.</summary>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    /// <param name="predicate">The database-translatable filter.</param>
    /// <param name="selector">The database-translatable projection.</param>
    /// <returns>The projected results.</returns>
    IEnumerable<TResult> SelectAll<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);

    /// <summary>Adds an entity.</summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    int Add(T entity, bool persist = true);
    /// <summary>Adds a range of entities.</summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    int AddRange(IEnumerable<T> entities, bool persist = true);
    /// <summary>Updates an entity.</summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    int Update(T entity, bool persist = true);
    /// <summary>Updates a range of entities.</summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    int UpdateRange(IEnumerable<T> entities, bool persist = true);
    /// <summary>Soft-deletes an entity.</summary>
    /// <param name="entity">The entity to soft-delete.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    int Delete(T entity, bool persist = true);
    /// <summary>Soft-deletes a range of entities.</summary>
    /// <param name="entities">The entities to soft-delete.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    int DeleteRange(IEnumerable<T> entities, bool persist = true);
    /// <summary>Finds and soft-deletes entities matching a predicate.</summary>
    /// <param name="predicate">The database-translatable filter.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    int DeleteRange(Expression<Func<T, bool>> predicate, bool persist = true);
    /// <summary>Persists pending changes.</summary>
    /// <returns>The number of state entries written.</returns>
    int SaveChanges();

    /// <summary>Finds an entity by identifier asynchronously.</summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The matching entity, or <see langword="null"/> when none exists.</returns>
    Task<T?> FindAsync(int id);
    /// <summary>Finds the first entity matching a predicate asynchronously.</summary>
    /// <param name="predicate">The database-translatable filter.</param>
    /// <returns>The first matching entity, or <see langword="null"/> when none exists.</returns>
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
    /// <summary>Finds all entities matching an optional predicate asynchronously.</summary>
    /// <param name="predicate">The optional database-translatable filter.</param>
    /// <returns>The matching, non-deleted entities.</returns>
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? predicate = null);
    /// <summary>Projects the entity with the specified identifier asynchronously.</summary>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    /// <param name="id">The entity identifier.</param>
    /// <param name="selector">The database-translatable projection.</param>
    /// <returns>The projected result, or <see langword="null"/> when none exists.</returns>
    Task<TResult?> SelectAsync<TResult>(int id, Expression<Func<T, TResult>> selector);
    /// <summary>Projects the first matching entity asynchronously.</summary>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    /// <param name="predicate">The database-translatable filter.</param>
    /// <param name="selector">The database-translatable projection.</param>
    /// <returns>The projected result, or <see langword="null"/> when none exists.</returns>
    Task<TResult?> SelectAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);
    /// <summary>Projects all entities asynchronously.</summary>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    /// <param name="selector">The database-translatable projection.</param>
    /// <returns>The projected, non-deleted results.</returns>
    Task<IEnumerable<TResult>> SelectAllAsync<TResult>(Expression<Func<T, TResult>> selector);
    /// <summary>Projects all matching entities asynchronously.</summary>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    /// <param name="predicate">The database-translatable filter.</param>
    /// <param name="selector">The database-translatable projection.</param>
    /// <returns>The projected results.</returns>
    Task<IEnumerable<TResult>> SelectAllAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector);

    /// <summary>Adds an entity asynchronously.</summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    Task<int> AddAsync(T entity, bool persist = true);
    /// <summary>Adds a range of entities asynchronously.</summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    Task<int> AddRangeAsync(IEnumerable<T> entities, bool persist = true);
    /// <summary>Updates an entity asynchronously.</summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    Task<int> UpdateAsync(T entity, bool persist = true);
    /// <summary>Updates a range of entities asynchronously.</summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    Task<int> UpdateRangeAsync(IEnumerable<T> entities, bool persist = true);
    /// <summary>Soft-deletes an entity asynchronously.</summary>
    /// <param name="entity">The entity to soft-delete.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    Task<int> DeleteAsync(T entity, bool persist = true);
    /// <summary>Soft-deletes a range of entities asynchronously.</summary>
    /// <param name="entities">The entities to soft-delete.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    Task<int> DeleteRangeAsync(IEnumerable<T> entities, bool persist = true);
    /// <summary>Finds and soft-deletes matching entities asynchronously.</summary>
    /// <param name="predicate">The database-translatable filter.</param>
    /// <param name="persist">Whether to save changes immediately.</param>
    /// <returns>The number of state entries written when persisted; otherwise zero.</returns>
    Task<int> DeleteRangeAsync(Expression<Func<T, bool>> predicate, bool persist = true);
    /// <summary>Persists pending changes asynchronously.</summary>
    /// <returns>The number of state entries written.</returns>
    Task<int> SaveChangesAsync();

    /// <summary>Determines whether any entity matches an optional predicate.</summary>
    /// <param name="predicate">The optional database-translatable filter.</param>
    /// <returns><see langword="true"/> when a matching entity exists.</returns>
    bool Any(Expression<Func<T, bool>>? predicate = null);
    /// <summary>Determines asynchronously whether any entity matches an optional predicate.</summary>
    /// <param name="predicate">The optional database-translatable filter.</param>
    /// <returns><see langword="true"/> when a matching entity exists.</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null);
    /// <summary>Counts entities matching an optional predicate.</summary>
    /// <param name="predicate">The optional database-translatable filter.</param>
    /// <returns>The number of matching entities.</returns>
    int Count(Expression<Func<T, bool>>? predicate = null);
    /// <summary>Counts entities matching an optional predicate asynchronously.</summary>
    /// <param name="predicate">The optional database-translatable filter.</param>
    /// <returns>The number of matching entities.</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}