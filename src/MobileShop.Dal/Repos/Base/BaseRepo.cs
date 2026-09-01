namespace MobileShop.Dal.Repos.Base;

public abstract class BaseRepo<T>(AppDbContext context) : IBaseRepo<T> where T : BaseEntity
{
    protected AppDbContext Context { get; } = context;
    protected DbSet<T> Table => Context.Set<T>();

    public virtual T? Find(int id) => Table.Find(id);
    public virtual IEnumerable<T> GetAll() => Table.ToList();

    public virtual int Add(T entity, bool persist = true)
    {
        Table.Add(entity);
        return persist ? SaveChanges() : 0;
    }

    public virtual int Update(T entity, bool persist = true)
    {
        Table.Update(entity);
        return persist ? SaveChanges() : 0;
    }

    // Soft delete - رکورد فیزیکی پاک نمیشه، فقط IsDeleted ست میشه؛ فیلتر سراسری تو AppDbContext خودش مخفیش می‌کنه
    public virtual int Delete(T entity, bool persist = true)
    {
        entity.IsDeleted = true;
        Table.Update(entity);
        return persist ? SaveChanges() : 0;
    }

    public int SaveChanges() => Context.SaveChanges();

    public virtual async Task<T?> FindAsync(int id) => await Table.FindAsync(id);
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await Table.ToListAsync();

    public virtual async Task<int> AddAsync(T entity, bool persist = true)
    {
        await Table.AddAsync(entity);
        return persist ? await SaveChangesAsync() : 0;
    }

    public virtual async Task<int> UpdateAsync(T entity, bool persist = true)
    {
        Table.Update(entity);
        return persist ? await SaveChangesAsync() : 0;
    }

    public virtual async Task<int> DeleteAsync(T entity, bool persist = true)
    {
        entity.IsDeleted = true;
        Table.Update(entity);
        return persist ? await SaveChangesAsync() : 0;
    }

    public async Task<int> SaveChangesAsync() => await Context.SaveChangesAsync();
}
