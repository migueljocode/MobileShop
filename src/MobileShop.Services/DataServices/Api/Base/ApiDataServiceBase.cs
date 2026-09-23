namespace MobileShop.Services.DataServices.Api.Base;

/// <summary>
/// Shared placeholder base for the Api-backed data services. It implements the whole
/// <see cref="IDataService{T}"/> surface, so the concrete <c>Api*DataService</c> classes only declare
/// the members that belong to their own interface.
/// </summary>
/// <remarks>
/// Every member throws <see cref="NotImplementedException"/>: these services exist so that
/// <c>UseApi: true</c> can boot and so the Api layer has a compiling surface, not to do work yet.
/// Replace the throws as the real Api wrappers are written.
/// </remarks>
/// <typeparam name="T">The entity the service manages.</typeparam>
public abstract class ApiDataServiceBase<T> : IDataService<T> where T : BaseEntity
{
    /// <summary>The exception message shared by every unimplemented member, naming the concrete service.</summary>
    private string NotImplementedMessage => $"{GetType().Name} is not implemented yet.";

    /// <inheritdoc />
    public virtual IEnumerable<T> FindAll(Expression<Func<T, bool>>? predicate = null)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? predicate = null)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual T? Find(int id)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual T? Find(Expression<Func<T, bool>> predicate)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual Task<T?> FindAsync(int id)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual bool Add(T entity)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual bool Update(T entity)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual bool Delete(int id)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual bool Delete(T entity)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual Task<bool> AddAsync(T entity)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual Task<bool> UpdateAsync(T entity)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual Task<bool> DeleteAsync(int id)
        => throw new NotImplementedException(NotImplementedMessage);

    /// <inheritdoc />
    public virtual Task<bool> DeleteAsync(T entity)
        => throw new NotImplementedException(NotImplementedMessage);
}

