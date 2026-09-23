namespace MobileShop.Dal.Initialization;

internal sealed class SampleDataSet
{
    public List<Person> People { get; init; } = [];
    public List<Seller> Sellers { get; init; } = [];
    public List<Customer> Customers { get; init; } = [];
    public List<User> Users { get; init; } = [];
    public List<Manufacturer> Manufacturers { get; init; } = [];
    public List<Category> Categories { get; init; } = [];
    public List<Color> Colors { get; init; } = [];
    public List<StorageCapacity> StorageCapacities { get; init; } = [];
    public List<Model> Models { get; init; } = [];
    public List<Product> Products { get; init; } = [];
    public List<Transaction> Transactions { get; init; } = [];
    public List<AppleId> AppleIds { get; init; } = [];
    public List<Phone> Phones { get; init; } = [];
    public List<SecondHand> SecondHands { get; init; } = [];
    public List<Guarantee> Guarantees { get; init; } = [];
    public List<DeviceSpec> DeviceSpecs { get; init; } = [];
    public List<Tablet> Tablets { get; init; } = [];
    public List<SmartWatch> SmartWatches { get; init; } = [];
    public List<Laptop> Laptops { get; init; } = [];
    public List<Cable> Cables { get; init; } = [];
    public List<Charger> Chargers { get; init; } = [];
    public List<PowerBank> PowerBanks { get; init; } = [];
    public List<PortableStorage> PortableStorages { get; init; } = [];
    public List<Case> Cases { get; init; } = [];
    public List<CaseModelFit> CaseModelFits { get; init; } = [];
    public List<Glass> Glasses { get; init; } = [];
    public List<GlassModelFit> GlassModelFits { get; init; } = [];
    public List<Employee> Employees { get; init; } = [];
}

internal static class SampleDataLoader
{
    private const string RelativePath = "Initialization/sample-data.json";
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static SampleDataSet Load()
    {
        var path = ResolvePath();
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;

        return new SampleDataSet
        {
            People = Deserialize<Person>(root, "people"),
            Sellers = Deserialize<Seller>(root, "sellers"),
            Customers = Deserialize<Customer>(root, "customers"),
            Users = Deserialize<User>(root, "users"),
            Manufacturers = Deserialize<Manufacturer>(root, "manufacturers"),
            Categories = Deserialize<Category>(root, "categories"),
            Colors = Deserialize<Color>(root, "colors"),
            StorageCapacities = Deserialize<StorageCapacity>(root, "storageCapacities"),
            Models = Deserialize<Model>(root, "models"),
            Products = Deserialize<Product>(root, "products"),
            Transactions = Deserialize<Transaction>(root, "transactions"),
            AppleIds = Deserialize<AppleId>(root, "appleIds"),
            Phones = Deserialize<Phone>(root, "phones"),
            SecondHands = Deserialize<SecondHand>(root, "secondHands"),
            Guarantees = Deserialize<Guarantee>(root, "guarantees"),
            DeviceSpecs = Deserialize<DeviceSpec>(root, "deviceSpecs"),
            Tablets = Deserialize<Tablet>(root, "tablets"),
            SmartWatches = Deserialize<SmartWatch>(root, "smartWatches"),
            Laptops = Deserialize<Laptop>(root, "laptops"),
            Cables = Deserialize<Cable>(root, "cables"),
            Chargers = Deserialize<Charger>(root, "chargers"),
            PowerBanks = Deserialize<PowerBank>(root, "powerBanks"),
            PortableStorages = Deserialize<PortableStorage>(root, "portableStorages"),
            Cases = Deserialize<Case>(root, "cases"),
            CaseModelFits = Deserialize<CaseModelFit>(root, "caseModelFits"),
            Glasses = Deserialize<Glass>(root, "glasses"),
            GlassModelFits = Deserialize<GlassModelFit>(root, "glassModelFits"),
            Employees = Deserialize<Employee>(root, "employees")
        };
    }

    private static List<TEntity> Deserialize<TEntity>(JsonElement root, string propertyName)
        => root.GetProperty(propertyName).Deserialize<List<TEntity>>(Options) ?? [];

    private static string ResolvePath()
    {
        var outputPath = Path.Combine(AppContext.BaseDirectory, RelativePath);
        if (File.Exists(outputPath))
        {
            return outputPath;
        }

        var assemblyPath = Path.GetDirectoryName(typeof(SampleDataLoader).Assembly.Location);
        var assemblyRelativePath = assemblyPath is null ? null : Path.Combine(assemblyPath, RelativePath);
        if (assemblyRelativePath is not null && File.Exists(assemblyRelativePath))
        {
            return assemblyRelativePath;
        }

        throw new FileNotFoundException($"Sample data file '{RelativePath}' was not copied to the application output.", outputPath);
    }
}
