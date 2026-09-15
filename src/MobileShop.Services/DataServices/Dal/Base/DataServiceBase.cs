namespace MobileShop.Services.DataServices.Dal.Base;

/// <summary>
/// Shared CRUD over <see cref="IBaseRepo{TEntity}"/> with structured logging.
/// <typeparamref name="TService"/> is the concrete service type (for <see cref="ILogger{TService}"/>).
/// </summary>
public abstract class DataServiceBase<TService, TEntity>(
    IBaseRepo<TEntity> repo,
    ILogger<TService> logger) : IDataService<TEntity>
    where TService : class
    where TEntity : BaseEntity
{
    protected IBaseRepo<TEntity> Repo { get; } = repo;
    protected ILogger<TService> Logger { get; } = logger;

    public virtual IEnumerable<TEntity> FindAll(Expression<Func<TEntity, bool>>? predicate = null)
        => Repo.FindAll(predicate);

    public virtual Task<IEnumerable<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null)
        => Repo.FindAllAsync(predicate);

    public virtual IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>>? predicate = null)
        => Repo.GetAll(predicate);

    public virtual Task<IEnumerable<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null)
        => Repo.GetAllAsync(predicate);

    public virtual TEntity? Find(int id) => Repo.Find(id);

    public virtual TEntity? Find(Expression<Func<TEntity, bool>> predicate)
        => Repo.Find(predicate);

    public virtual Task<TEntity?> FindAsync(int id) => Repo.FindAsync(id);

    public virtual Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate)
        => Repo.FindAsync(predicate);

    public virtual bool Add(TEntity entity)
    {
        var result = Repo.Add(entity) > 0;
        if (result)
            Logger.LogInformation("Added {EntityType} Id={Id}", typeof(TEntity).Name, entity.Id);
        else
            Logger.LogWarning("Failed to add {EntityType}", typeof(TEntity).Name);
        return result;
    }

    public virtual bool Update(TEntity entity)
    {
        var result = Repo.Update(entity) > 0;
        if (result)
            Logger.LogInformation("Updated {EntityType} Id={Id}", typeof(TEntity).Name, entity.Id);
        else
            Logger.LogWarning("Failed to update {EntityType} Id={Id}", typeof(TEntity).Name, entity.Id);
        return result;
    }

    public virtual bool Delete(TEntity entity)
    {
        var result = Repo.Delete(entity) > 0;
        if (result)
            Logger.LogInformation("Soft-deleted {EntityType} Id={Id}", typeof(TEntity).Name, entity.Id);
        else
            Logger.LogWarning("Failed to delete {EntityType} Id={Id}", typeof(TEntity).Name, entity.Id);
        return result;
    }

    public virtual bool Delete(int id)
    {
        var entity = Repo.Find(id);
        if (entity is null)
        {
            Logger.LogWarning("Delete failed: {EntityType} Id={Id} not found", typeof(TEntity).Name, id);
            return false;
        }
        return Delete(entity);
    }

    public virtual async Task<bool> AddAsync(TEntity entity)
    {
        var result = await Repo.AddAsync(entity) > 0;
        if (result)
            Logger.LogInformation("Added {EntityType} Id={Id}", typeof(TEntity).Name, entity.Id);
        else
            Logger.LogWarning("Failed to add {EntityType}", typeof(TEntity).Name);
        return result;
    }

    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        var result = await Repo.UpdateAsync(entity) > 0;
        if (result)
            Logger.LogInformation("Updated {EntityType} Id={Id}", typeof(TEntity).Name, entity.Id);
        else
            Logger.LogWarning("Failed to update {EntityType} Id={Id}", typeof(TEntity).Name, entity.Id);
        return result;
    }

    public virtual async Task<bool> DeleteAsync(TEntity entity)
    {
        var result = await Repo.DeleteAsync(entity) > 0;
        if (result)
            Logger.LogInformation("Soft-deleted {EntityType} Id={Id}", typeof(TEntity).Name, entity.Id);
        else
            Logger.LogWarning("Failed to delete {EntityType} Id={Id}", typeof(TEntity).Name, entity.Id);
        return result;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await Repo.FindAsync(id);
        if (entity is null)
        {
            Logger.LogWarning("Delete failed: {EntityType} Id={Id} not found", typeof(TEntity).Name, id);
            return false;
        }
        return await DeleteAsync(entity);
    }
}