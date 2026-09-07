namespace MobileShop.Dal.Repos.Base;

/// <inheritdoc cref="IBaseRepo{T}" />
public abstract class BaseRepo<T>(AppDbContext context) : IBaseRepo<T> where T : BaseEntity
{
    protected AppDbContext Context { get; } = context;
    protected DbSet<T> Table => Context.Set<T>();

    /// <inheritdoc />
    public virtual T? Find(int id) => Table.Find(id);

    /// <inheritdoc />
    public virtual IEnumerable<T> GetAll() => Table.ToList();

    /// <inheritdoc />
    public virtual int Add(T entity, bool persist = true)
    {
        Table.Add(entity);
        return persist ? SaveChanges() : 0;
    }

    /// <inheritdoc />
    public virtual int Update(T entity, bool persist = true)
    {
        Table.Update(entity);
        return persist ? SaveChanges() : 0;
    }

    /// <inheritdoc />
    public virtual int Delete(T entity, bool persist = true)
    {
        entity.IsDeleted = true;
        Table.Update(entity);
        return persist ? SaveChanges() : 0;
    }

    /// <inheritdoc />
    public int SaveChanges() => Context.SaveChanges();

    /// <inheritdoc />
    public virtual async Task<T?> FindAsync(int id) => await Table.FindAsync(id);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await Table.ToListAsync();

    /// <inheritdoc />
    public virtual async Task<int> AddAsync(T entity, bool persist = true)
    {
        await Table.AddAsync(entity);
        return persist ? await SaveChangesAsync() : 0;
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateAsync(T entity, bool persist = true)
    {
        Table.Update(entity);
        return persist ? await SaveChangesAsync() : 0;
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteAsync(T entity, bool persist = true)
    {
        entity.IsDeleted = true;
        Table.Update(entity);
        return persist ? await SaveChangesAsync() : 0;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync() => await Context.SaveChangesAsync();
}
