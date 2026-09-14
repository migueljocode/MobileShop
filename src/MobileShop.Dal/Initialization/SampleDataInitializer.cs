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
        context.IPhones.IgnoreQueryFilters().ExecuteDelete();
        context.Guarantees.IgnoreQueryFilters().ExecuteDelete();
        context.SecondHands.IgnoreQueryFilters().ExecuteDelete();
        context.AppleIds.IgnoreQueryFilters().ExecuteDelete();
        context.Phones.IgnoreQueryFilters().ExecuteDelete();
        context.Transactions.IgnoreQueryFilters().ExecuteDelete();
        context.Products.IgnoreQueryFilters().ExecuteDelete();
        context.Users.IgnoreQueryFilters().ExecuteDelete();
        context.Customers.IgnoreQueryFilters().ExecuteDelete();
        context.Sellers.IgnoreQueryFilters().ExecuteDelete();
        context.People.IgnoreQueryFilters().ExecuteDelete();

        // SQLite tracks its own autoincrement counters in sqlite_sequence - reset them so re-seeded IDs start clean
        context.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence");
    }

    internal static void SeedData(AppDbContext context)
    {
        try
        {
            ProcessInsert(context, context.People, SampleData.People);
            ProcessInsert(context, context.Sellers, SampleData.Sellers);
            ProcessInsert(context, context.Customers, SampleData.Customers);
            ProcessInsert(context, context.Users, SampleData.Users);
            ProcessInsert(context, context.Products, SampleData.Products);
            ProcessInsert(context, context.Transactions, SampleData.Transactions);
            ProcessInsert(context, context.AppleIds, SampleData.AppleIds);
            ProcessInsert(context, context.Phones, SampleData.Phones);
            ProcessInsert(context, context.SecondHands, SampleData.SecondHands);
            ProcessInsert(context, context.Guarantees, SampleData.Guarantees);
            ProcessInsert(context, context.IPhones, SampleData.IPhones);
        }
        catch (Exception ex)
        {
            // i think exception should be thrown to catch by serilog in upper layer.
            Console.WriteLine(ex);
            throw;
        }

        // SQLite lets you insert explicit values into an integer primary key directly - unlike SQL
        // Server, there's no IDENTITY_INSERT ceremony needed to seed rows with fixed, known Ids
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
