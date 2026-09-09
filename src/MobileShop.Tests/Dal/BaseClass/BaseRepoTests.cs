namespace MobileShop.Tests.Dal.BaseClass;

/// <summary>
/// Generic CRUD test suite for any <see cref="IBaseRepo{T}"/> implementation. A concrete test
/// class inherits this, implements <see cref="CreateRepo"/> and <see cref="CreateValidEntity"/>
/// for its entity type, and automatically gets coverage for every method on
/// <see cref="IBaseRepo{T}"/> - it only needs to add tests for whatever custom methods its own
/// repo interface adds beyond that contract.
/// </summary>
/// <typeparam name="TEntity">The entity type under test.</typeparam>
/// <typeparam name="TRepo">The repository interface under test.</typeparam>
public abstract class BaseRepoTests<TEntity, TRepo> : RepoTestBase
    where TEntity : BaseEntity
    where TRepo : IBaseRepo<TEntity>
{
    /// <summary>Creates the repo under test, wired to the shared <see cref="RepoTestBase.Context"/>.</summary>
    protected abstract TRepo CreateRepo();

    /// <summary>
    /// Creates a new, valid, not-yet-persisted entity - filling in whatever required fields and
    /// FK dependencies that entity type needs (saving any parent rows directly via <see cref="RepoTestBase.Context"/>).
    /// </summary>
    protected abstract TEntity CreateValidEntity();

    [Fact]
    public void Find_ReturnsEntity_WhenExists()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);

        var found = repo.Find(entity.Id);

        Assert.NotNull(found);
        Assert.Equal(entity.Id, found!.Id);
    }

    [Fact]
    public void Find_ReturnsNull_WhenNotExists()
        => Assert.Null(CreateRepo().Find(-1));

    [Fact]
    public async Task FindAsync_ReturnsEntity_WhenExists()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        await repo.AddAsync(entity);

        var found = await repo.FindAsync(entity.Id);

        Assert.NotNull(found);
        Assert.Equal(entity.Id, found!.Id);
    }

    [Fact]
    public async Task FindAsync_ReturnsNull_WhenNotExists()
        => Assert.Null(await CreateRepo().FindAsync(-1));

    [Fact]
    public void GetAll_ReturnsEmpty_WhenNoData()
        => Assert.Empty(CreateRepo().GetAll());

    [Fact]
    public void GetAll_ReturnsAllEntities_WhenDataExists()
    {
        var repo = CreateRepo();
        repo.Add(CreateValidEntity());
        repo.Add(CreateValidEntity());

        Assert.Equal(2, repo.GetAll().Count());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities_WhenDataExists()
    {
        var repo = CreateRepo();
        await repo.AddAsync(CreateValidEntity());
        await repo.AddAsync(CreateValidEntity());

        Assert.Equal(2, (await repo.GetAllAsync()).Count());
    }

    [Fact]
    public void Add_SavesWhenPersistTrue()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();

        repo.Add(entity, persist: true);

        Assert.NotEqual(0, entity.Id);
        Assert.Equal(EntityState.Unchanged, Context.Entry(entity).State);
    }

    [Fact]
    public void Add_DoesNotSaveWhenPersistFalse()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();

        repo.Add(entity, persist: false);

        Assert.Equal(EntityState.Added, Context.Entry(entity).State);
    }

    [Fact]
    public async Task AddAsync_SavesWhenPersistTrue()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();

        await repo.AddAsync(entity, persist: true);

        Assert.NotEqual(0, entity.Id);
        Assert.Equal(EntityState.Unchanged, Context.Entry(entity).State);
    }

    [Fact]
    public async Task AddAsync_DoesNotSaveWhenPersistFalse()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();

        await repo.AddAsync(entity, persist: false);

        Assert.Equal(EntityState.Added, Context.Entry(entity).State);
    }

    [Fact]
    public void Update_MarksEntityUnchangedAfterSave()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);

        repo.Update(entity);

        Assert.Equal(EntityState.Unchanged, Context.Entry(entity).State);
    }

    [Fact]
    public async Task UpdateAsync_MarksEntityUnchangedAfterSave()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        await repo.AddAsync(entity);

        await repo.UpdateAsync(entity);

        Assert.Equal(EntityState.Unchanged, Context.Entry(entity).State);
    }

    [Fact]
    public void Delete_SoftDeletesAndHidesFromQueries()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity);

        repo.Delete(entity);

        Assert.True(entity.IsDeleted);
        Assert.Null(repo.Find(entity.Id));
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesAndHidesFromQueries()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        await repo.AddAsync(entity);

        await repo.DeleteAsync(entity);

        Assert.True(entity.IsDeleted);
        Assert.Null(await repo.FindAsync(entity.Id));
        Assert.Empty(await repo.GetAllAsync());
    }

    [Fact]
    public void SaveChanges_PersistsPendingChanges()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        repo.Add(entity, persist: false);

        repo.SaveChanges();

        Assert.Equal(EntityState.Unchanged, Context.Entry(entity).State);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsPendingChanges()
    {
        var repo = CreateRepo();
        var entity = CreateValidEntity();
        await repo.AddAsync(entity, persist: false);

        await repo.SaveChangesAsync();

        Assert.Equal(EntityState.Unchanged, Context.Entry(entity).State);
    }
}
