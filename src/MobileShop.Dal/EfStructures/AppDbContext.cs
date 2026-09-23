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
    public virtual DbSet<Guarantee> Guarantees => Set<Guarantee>();
    public virtual DbSet<User> Users => Set<User>();
    public virtual DbSet<Employee> Employees => Set<Employee>();

    // lookups
    public virtual DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public virtual DbSet<Model> Models => Set<Model>();
    public virtual DbSet<Category> Categories => Set<Category>();
    public virtual DbSet<Color> Colors => Set<Color>();
    public virtual DbSet<StorageCapacity> StorageCapacities => Set<StorageCapacity>();

    // product profiles (all 1:1 with Product)
    public virtual DbSet<DeviceSpec> DeviceSpecs => Set<DeviceSpec>();
    public virtual DbSet<AppleInfo> AppleInfos => Set<AppleInfo>();
    public virtual DbSet<Tablet> Tablets => Set<Tablet>();
    public virtual DbSet<SmartWatch> SmartWatches => Set<SmartWatch>();
    public virtual DbSet<Laptop> Laptops => Set<Laptop>();
    public virtual DbSet<Cable> Cables => Set<Cable>();
    public virtual DbSet<Charger> Chargers => Set<Charger>();
    public virtual DbSet<PowerBank> PowerBanks => Set<PowerBank>();
    public virtual DbSet<PortableStorage> PortableStorages => Set<PortableStorage>();
    public virtual DbSet<Case> Cases => Set<Case>();
    public virtual DbSet<Glass> Glasses => Set<Glass>();

    // explicit many-to-many joins
    public virtual DbSet<CaseModelFit> CaseModelFits => Set<CaseModelFit>();
    public virtual DbSet<GlassModelFit> GlassModelFits => Set<GlassModelFit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseEntity).Assembly);

        modelBuilder.ApplySoftDeleteForEntities();
    }
}
