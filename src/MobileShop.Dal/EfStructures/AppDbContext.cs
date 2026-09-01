namespace MobileShop.Dal.EfStructures;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public virtual DbSet<Phone> Phones => Set<Phone>();
    public virtual DbSet<StockGood> StockGoods => Set<StockGood>();
    public virtual DbSet<Purchase> Purchases => Set<Purchase>();
    public virtual DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public virtual DbSet<Sale> Sales => Set<Sale>();
    public virtual DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public virtual DbSet<Seller> Sellers => Set<Seller>();
    public virtual DbSet<Customer> Customers => Set<Customer>();
    public virtual DbSet<AppleId> AppleIds => Set<AppleId>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoodBase).Assembly);

        // فیلتر سراسری soft-delete برای همه‌ی انتیتی‌هایی که از BaseEntity میان - یه‌بار همینجا، نه تکراری تو هر Configuration
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
