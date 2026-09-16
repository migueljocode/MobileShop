namespace MobileShop.Dal.Initialization;

internal sealed class SampleDataSet
{
    public List<Person> People { get; init; } = [];
    public List<Seller> Sellers { get; init; } = [];
    public List<Customer> Customers { get; init; } = [];
    public List<User> Users { get; init; } = [];
    public List<Product> Products { get; init; } = [];
    public List<Transaction> Transactions { get; init; } = [];
    public List<AppleId> AppleIds { get; init; } = [];
    public List<Phone> Phones { get; init; } = [];
    public List<SecondHand> SecondHands { get; init; } = [];
    public List<Guarantee> Guarantees { get; init; } = [];
    public List<IPhone> IPhones { get; init; } = [];
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
            Products = Deserialize<Product>(root, "products"),
            Transactions = Deserialize<Transaction>(root, "transactions"),
            AppleIds = Deserialize<AppleId>(root, "appleIds"),
            Phones = Deserialize<Phone>(root, "phones"),
            SecondHands = Deserialize<SecondHand>(root, "secondHands"),
            Guarantees = Deserialize<Guarantee>(root, "guarantees"),
            IPhones = Deserialize<IPhone>(root, "iPhones")
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
