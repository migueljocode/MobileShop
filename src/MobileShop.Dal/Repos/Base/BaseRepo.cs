namespace MobileShop.Dal.Repos.Base;

/// <inheritdoc cref="IBaseRepo{T}" />
public abstract class BaseRepo<T>(AppDbContext context) : IBaseRepo<T> where T : BaseEntity
{
    protected AppDbContext Context { get; } = context;
    protected DbSet<T> Table => Context.Set<T>();

    public virtual T? Find(int id) => Table.FirstOrDefault(e => e.Id == id);

    public virtual T? Find(Expression<Func<T, bool>> predicate) => Table.FirstOrDefault(predicate);

    public virtual IEnumerable<T> FindAll(Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = Table;
        if (predicate is not null)
            query = query.Where(predicate);
        return query.ToList();
    }

    public virtual TResult? Select<TResult>(int id, Expression<Func<T, TResult>> selector)
        => Table.Where(e => e.Id == id).Select(selector).FirstOrDefault();

    public virtual TResult? Select<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        => Table.Where(predicate).Select(selector).FirstOrDefault();

    public virtual IEnumerable<TResult> SelectAll<TResult>(Expression<Func<T, TResult>> selector)
        => Table.Select(selector).ToList();

    public virtual IEnumerable<TResult> SelectAll<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        => Table.Where(predicate).Select(selector).ToList();

    public virtual int Add(T entity, bool persist = true)
    {
        Table.Add(entity);
        return persist ? SaveChanges() : 0;
    }

    public virtual int AddRange(IEnumerable<T> entities, bool persist = true)
    {
        Table.AddRange(entities);
        return persist ? SaveChanges() : 0;
    }

    public virtual int Update(T entity, bool persist = true)
    {
        Table.Update(entity);
        return persist ? SaveChanges() : 0;
    }

    public virtual int UpdateRange(IEnumerable<T> entities, bool persist = true)
    {
        Table.UpdateRange(entities);
        return persist ? SaveChanges() : 0;
    }

    public virtual int Delete(T entity, bool persist = true)
    {
        entity.IsDeleted = true;
        Table.Update(entity);
        return persist ? SaveChanges() : 0;
    }

    public virtual int DeleteRange(IEnumerable<T> entities, bool persist = true)
    {
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
        }

        Table.UpdateRange(entities);
        return persist ? SaveChanges() : 0;
    }

    public virtual int DeleteRange(Expression<Func<T, bool>> predicate, bool persist = true)
    {
        var entities = Table.Where(predicate).ToList();
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
        }

        Table.UpdateRange(entities);
        return persist ? SaveChanges() : 0;
    }

    public int SaveChanges() => Context.SaveChanges();

    public virtual async Task<T?> FindAsync(int id) => await Table.FirstOrDefaultAsync(e => e.Id == id);

    public virtual async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
        => await Table.FirstOrDefaultAsync(predicate);

    public virtual async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = Table;
        if (predicate is not null)
            query = query.Where(predicate);
        return await query.ToListAsync();
    }

    public virtual async Task<TResult?> SelectAsync<TResult>(int id, Expression<Func<T, TResult>> selector)
        => await Table.Where(e => e.Id == id).Select(selector).FirstOrDefaultAsync();

    public virtual async Task<TResult?> SelectAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        => await Table.Where(predicate).Select(selector).FirstOrDefaultAsync();

    public virtual async Task<IEnumerable<TResult>> SelectAllAsync<TResult>(Expression<Func<T, TResult>> selector)
        => await Table.Select(selector).ToListAsync();

    public virtual async Task<IEnumerable<TResult>> SelectAllAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector)
        => await Table.Where(predicate).Select(selector).ToListAsync();

    public virtual async Task<int> AddAsync(T entity, bool persist = true)
    {
        await Table.AddAsync(entity);
        return persist ? await SaveChangesAsync() : 0;
    }

    public virtual async Task<int> AddRangeAsync(IEnumerable<T> entities, bool persist = true)
    {
        await Table.AddRangeAsync(entities);
        return persist ? await SaveChangesAsync() : 0;
    }

    public virtual async Task<int> UpdateAsync(T entity, bool persist = true)
    {
        Table.Update(entity);
        return persist ? await SaveChangesAsync() : 0;
    }

    public virtual async Task<int> UpdateRangeAsync(IEnumerable<T> entities, bool persist = true)
    {
        Table.UpdateRange(entities);
        return persist ? await SaveChangesAsync() : 0;
    }

    public virtual async Task<int> DeleteAsync(T entity, bool persist = true)
    {
        entity.IsDeleted = true;
        Table.Update(entity);
        return persist ? await SaveChangesAsync() : 0;
    }

    public virtual async Task<int> DeleteRangeAsync(IEnumerable<T> entities, bool persist = true)
    {
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
        }

        Table.UpdateRange(entities);
        return persist ? await SaveChangesAsync() : 0;
    }

    public virtual async Task<int> DeleteRangeAsync(Expression<Func<T, bool>> predicate, bool persist = true)
    {
        var entities = await Table.Where(predicate).ToListAsync();
        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
        }

        Table.UpdateRange(entities);
        return persist ? await SaveChangesAsync() : 0;
    }

    public async Task<int> SaveChangesAsync() => await Context.SaveChangesAsync();

    public virtual bool Any(Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = Table;
        return predicate is null ? query.Any() : query.Any(predicate);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = Table;
        return predicate is null ? await query.AnyAsync() : await query.AnyAsync(predicate);
    }

    public virtual int Count(Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = Table;
        return predicate is null ? query.Count() : query.Count(predicate);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = Table;
        return predicate is null ? await query.CountAsync() : await query.CountAsync(predicate);
    }

    public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null) => FindAll(predicate);

    public virtual Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null) => FindAllAsync(predicate);
}