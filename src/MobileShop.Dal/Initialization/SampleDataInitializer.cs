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
        // FK-safe order (dependents first). IgnoreQueryFilters so soft-deleted rows go too.
        ClearSet<Guarantee>(context);
        ClearSet<SecondHand>(context);
        ClearSet<AppleId>(context);
        ClearSet<Phone>(context);
        ClearSet<Transaction>(context);
        ClearSet<Product>(context);
        ClearSet<User>(context);
        ClearSet<Customer>(context);
        ClearSet<Seller>(context);
        ClearSet<Person>(context);

        context.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence");
        
        static void ClearSet<TEntity>(AppDbContext context) where TEntity : class
            => context.Set<TEntity>().IgnoreQueryFilters().ExecuteDelete();
    }


    internal static void SeedData(AppDbContext context)
    {
        var data = SampleDataLoader.Load();

        ProcessInsert(context, context.People, data.People);
        ProcessInsert(context, context.Sellers, data.Sellers);
        ProcessInsert(context, context.Customers, data.Customers);
        ProcessInsert(context, context.Users, data.Users);
        ProcessInsert(context, context.Products, data.Products);
        ProcessInsert(context, context.Transactions, data.Transactions);
        ProcessInsert(context, context.AppleIds, data.AppleIds);
        ProcessInsert(context, context.Phones, data.Phones);
        ProcessInsert(context, context.SecondHands, data.SecondHands);
        ProcessInsert(context, context.Guarantees, data.Guarantees);
        
        static void ProcessInsert<TEntity>(
            AppDbContext context,
            DbSet<TEntity> table,
            IReadOnlyList<TEntity> records) where TEntity : BaseEntity
        {
            if (table.Any())
                return;

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
