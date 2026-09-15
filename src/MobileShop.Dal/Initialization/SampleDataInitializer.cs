namespace MobileShop.Dal.Initialization;

public static class SampleDataInitializer
{
    internal static void DropAndCreateDatabase(AppDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.Migrate();
    }

    internal static void ClearData(AppDbContext context)
    {
        // IgnoreQueryFilters() matters here - without it, the soft-delete filter would leave
        // already-deleted rows behind, and this is meant to wipe everything for a clean reseed.
        // Deepest-dependency-first, mirroring the FK graph.
        var deleteOperations = new Action[]
        {
            () => context.IPhones.IgnoreQueryFilters().ExecuteDelete(),
            () => context.Guarantees.IgnoreQueryFilters().ExecuteDelete(),
            () => context.SecondHands.IgnoreQueryFilters().ExecuteDelete(),
            () => context.AppleIds.IgnoreQueryFilters().ExecuteDelete(),
            () => context.Phones.IgnoreQueryFilters().ExecuteDelete(),
            () => context.Transactions.IgnoreQueryFilters().ExecuteDelete(),
            () => context.Products.IgnoreQueryFilters().ExecuteDelete(),
            () => context.Users.IgnoreQueryFilters().ExecuteDelete(),
            () => context.Customers.IgnoreQueryFilters().ExecuteDelete(),
            () => context.Sellers.IgnoreQueryFilters().ExecuteDelete(),
            () => context.People.IgnoreQueryFilters().ExecuteDelete()
        };

        foreach (var delete in deleteOperations)
        {
            delete();
        }

        // SQLite tracks its own autoincrement counters in sqlite_sequence - reset them so re-seeded IDs start clean
        context.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence");
    }

    internal static void SeedData(AppDbContext context)
    {
        var data = SampleDataLoader.Load();
        var seedOperations = new Action[]
        {
            () => ProcessInsert(context, context.People, data.People),
            () => ProcessInsert(context, context.Sellers, data.Sellers),
            () => ProcessInsert(context, context.Customers, data.Customers),
            () => ProcessInsert(context, context.Users, data.Users),
            () => ProcessInsert(context, context.Products, data.Products),
            () => ProcessInsert(context, context.Transactions, data.Transactions),
            () => ProcessInsert(context, context.AppleIds, data.AppleIds),
            () => ProcessInsert(context, context.Phones, data.Phones),
            () => ProcessInsert(context, context.SecondHands, data.SecondHands),
            () => ProcessInsert(context, context.Guarantees, data.Guarantees),
            () => ProcessInsert(context, context.IPhones, data.IPhones)
        };

        foreach (var seed in seedOperations)
        {
            seed();
        }

        static void ProcessInsert<TEntity>(AppDbContext context, DbSet<TEntity> table, List<TEntity> records) where TEntity : BaseEntity
        {
            if (table.Any())
            {
                return;
            }

            table.AddRange(records);
            context.SaveChanges();
        }
    }

    public static void InitializeData(AppDbContext context)
    {
        DropAndCreateDatabase(context);
        SeedData(context);
    }

    public static void ClearAndReseedDatabase(AppDbContext context)
    {
        ClearData(context);
        SeedData(context);
    }

    /// <summary>
    /// Non-destructive seeding for app startup - applies the bundled sample data only when the
    /// database is empty, so re-runs never wipe existing rows. Called from the Web app's startup.
    /// </summary>
    public static void SeedIfEmpty(AppDbContext context)
    {
        if (!context.People.Any())
        {
            SeedData(context);
        }
    }
}
