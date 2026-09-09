namespace MobileShop.Dal.Repos.Base;

/// <summary>
/// Generic CRUD repository contract shared by every entity repository.
/// Deletes are soft deletes - see <see cref="Delete"/>.
/// </summary>
/// <typeparam name="T">The entity type, which must derive from <see cref="BaseEntity"/>.</typeparam>
public interface IBaseRepo<T> where T : BaseEntity
{
    /// <summary>
    /// Finds an entity by its primary key. Always reflects the current query filter state (a
    /// soft-deleted entity is never returned), even if that entity is already tracked in this
    /// context - unlike <c>DbSet.Find</c> itself, which would return it anyway.
    /// </summary>
    /// <param name="id">The entity's Id.</param>
    /// <returns>The entity, or <see langword="null"/> if no match is found.</returns>
    T? Find(int id);

    /// <summary>Returns every entity of this type (soft-deleted rows are excluded automatically).</summary>
    /// <returns>All matching entities.</returns>
    IEnumerable<T> GetAll();

    /// <summary>Marks a new entity for insertion.</summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="persist">If <see langword="true"/>, calls <see cref="SaveChanges"/> immediately.</param>
    /// <returns>The number of rows written, or 0 if <paramref name="persist"/> is <see langword="false"/>.</returns>
    int Add(T entity, bool persist = true);

    /// <summary>Marks an existing entity as modified.</summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="persist">If <see langword="true"/>, calls <see cref="SaveChanges"/> immediately.</param>
    /// <returns>The number of rows written, or 0 if <paramref name="persist"/> is <see langword="false"/>.</returns>
    int Update(T entity, bool persist = true);

    /// <summary>
    /// Soft-deletes an entity by setting <see cref="BaseEntity.IsDeleted"/> to <see langword="true"/> -
    /// the row is never physically removed.
    /// </summary>
    /// <param name="entity">The entity to soft-delete.</param>
    /// <param name="persist">If <see langword="true"/>, calls <see cref="SaveChanges"/> immediately.</param>
    /// <returns>The number of rows written, or 0 if <paramref name="persist"/> is <see langword="false"/>.</returns>
    int Delete(T entity, bool persist = true);

    /// <summary>Persists all pending changes tracked by the underlying context.</summary>
    /// <returns>The number of rows written.</returns>
    int SaveChanges();

    /// <summary>Asynchronous version of <see cref="Find"/>.</summary>
    /// <param name="id">The entity's Id.</param>
    /// <returns>The entity, or <see langword="null"/> if no match is found.</returns>
    Task<T?> FindAsync(int id);

    /// <summary>Asynchronous version of <see cref="GetAll"/>.</summary>
    /// <returns>All matching entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>Asynchronous version of <see cref="Add"/>.</summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="persist">If <see langword="true"/>, calls <see cref="SaveChangesAsync"/> immediately.</param>
    /// <returns>The number of rows written, or 0 if <paramref name="persist"/> is <see langword="false"/>.</returns>
    Task<int> AddAsync(T entity, bool persist = true);

    /// <summary>Asynchronous version of <see cref="Update"/>.</summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="persist">If <see langword="true"/>, calls <see cref="SaveChangesAsync"/> immediately.</param>
    /// <returns>The number of rows written, or 0 if <paramref name="persist"/> is <see langword="false"/>.</returns>
    Task<int> UpdateAsync(T entity, bool persist = true);

    /// <summary>Asynchronous version of <see cref="Delete"/> - also a soft delete, not a physical removal.</summary>
    /// <param name="entity">The entity to soft-delete.</param>
    /// <param name="persist">If <see langword="true"/>, calls <see cref="SaveChangesAsync"/> immediately.</param>
    /// <returns>The number of rows written, or 0 if <paramref name="persist"/> is <see langword="false"/>.</returns>
    Task<int> DeleteAsync(T entity, bool persist = true);

    /// <summary>Asynchronous version of <see cref="SaveChanges"/>.</summary>
    /// <returns>The number of rows written.</returns>
    Task<int> SaveChangesAsync();
}