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

        modelBuilder.ApplySoftDeleteQueryFilters();
    }
}
