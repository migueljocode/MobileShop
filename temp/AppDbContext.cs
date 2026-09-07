namespace MobileShop.Dal.EfStructures;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public virtual DbSet<Person> People => Set<Person>();
    public virtual DbSet<Seller> Sellers => Set<Seller>();
    public virtual DbSet<Customer> Customers => Set<Customer>();
    public virtual DbSet<Transaction> Transactions => Set<Transaction>();
    public virtual DbSet<Product> Products => Set<Product>();
    public virtual DbSet<AppleId> AppleIds => Set<AppleId>();
    public virtual DbSet<Phone> Phones => Set<Phone>();
    public virtual DbSet<SecondHand> SecondHands => Set<SecondHand>();
    public virtual DbSet<IPhone> IPhones => Set<IPhone>();
    public virtual DbSet<Guarantee> Guarantees => Set<Guarantee>();
    public virtual DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseEntity).Assembly);

        // global soft-delete filter for every entity type that derives from BaseEntity - one place, not repeated per Configuration class
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(isDeletedProperty), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    public override int SaveChanges()
    {
        try
        {
            return base.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new CustomConcurrencyException("رکورد توسط یه عملیات دیگه تغییر کرده بود.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new CustomDbUpdateException("ذخیره‌سازی توی دیتابیس با خطا مواجه شد.", ex);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new CustomConcurrencyException("رکورد توسط یه عملیات دیگه تغییر کرده بود.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new CustomDbUpdateException("ذخیره‌سازی توی دیتابیس با خطا مواجه شد.", ex);
        }
    }
}
