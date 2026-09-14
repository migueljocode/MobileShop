namespace MobileShop.Dal.Initialization;

internal static class SampleData
{
    // Id 1 is the shop's own sentinel Person - it carries a Seller profile (used as the seller
    // on sale-leg transactions) and a Customer profile (used as the customer on purchase-leg ones)
    internal static List<Person> People { get; } =
    [
        new() { Id = 1, FirstName = "Mohammad", LastName = "Karimi", PhoneNumber = "09120000001", Notes = "Shop owner" },
        new() { Id = 2, FirstName = "Ali", LastName = "Rezaei", PhoneNumber = "09120000002", Notes = "Regular supplier" },
        new() { Id = 3, FirstName = "Sara", LastName = "Ahmadi", PhoneNumber = "09120000003" }
    ];

    internal static List<Seller> Sellers { get; } =
    [
        new() { Id = 1, PersonId = 1, EntityType = SellerEntityType.Real },
        new() { Id = 2, PersonId = 2, EntityType = SellerEntityType.Real }
    ];

    internal static List<Customer> Customers { get; } =
    [
        new() { Id = 1, PersonId = 1, NationalId = "0000000000" },
        new() { Id = 2, PersonId = 3, NationalId = "0012345678" }
    ];

    internal static List<User> Users { get; } =
    [
        new() { Id = 1, PersonId = 1, Username = "admin", PasswordHash = "REPLACE_WITH_REAL_HASH" }
    ];

    internal static List<Product> Products { get; } =
    [
        new() { Id = 1, Price = 45_000_000, Manufacturer = "Apple", Model = "iPhone 13 128GB" },
        new() { Id = 2, Price = 32_000_000, Manufacturer = "Samsung", Model = "Galaxy S24" },
        new() { Id = 3, Price = 18_000_000, Manufacturer = "Apple", Model = "iPhone 11 (Used)" },
        new() { Id = 4, Price = 1_200_000, Manufacturer = "Anker", Model = "20W USB-C Charger" }
    ];

    internal static List<Transaction> Transactions { get; } =
    [
        new() { Id = 1, Date = new DateTime(2026, 1, 10), SellerId = 2, CustomerId = 1, ProductId = 1, FinishedPrice = 42_000_000, Direction = TransactionDirection.Buy },
        new() { Id = 2, Date = new DateTime(2026, 1, 15), SellerId = 1, CustomerId = 2, ProductId = 1, FinishedPrice = 45_000_000, Direction = TransactionDirection.Sell },
        new() { Id = 3, Date = new DateTime(2026, 1, 12), SellerId = 2, CustomerId = 1, ProductId = 2, FinishedPrice = 29_000_000, Direction = TransactionDirection.Buy },
        new() { Id = 4, Date = new DateTime(2026, 1, 5), SellerId = 2, CustomerId = 1, ProductId = 3, FinishedPrice = 15_000_000, Direction = TransactionDirection.Buy },
        new() { Id = 5, Date = new DateTime(2026, 1, 20), SellerId = 2, CustomerId = 1, ProductId = 4, FinishedPrice = 900_000, Direction = TransactionDirection.Buy }
    ];

    internal static List<AppleId> AppleIds { get; } =
    [
        new() { Id = 1, Email = "shop.appleid1@example.com", Password = "samplepass1", Notes = "Comes with iPhone 13", ProductId = 1 }
    ];

    internal static List<Phone> Phones { get; } =
    [
        new() { Id = 1, IMEI1 = "123456789012345", Color = "Midnight", ProductId = 1 },
        new() { Id = 2, IMEI1 = "234567890123456", IMEI2 = "234567890123457", Color = "Black", ProductId = 2 },
        new() { Id = 3, IMEI1 = "345678901234567", Color = "Green", OwnershipTransferred = true, ProductId = 3 }
    ];

    internal static List<SecondHand> SecondHands { get; } =
    [
        new() { Id = 1, TestPeriodDays = 7, UsedDurationDays = 730, ProductId = 3 }
    ];

    internal static List<Guarantee> Guarantees { get; } =
    [
        new() { Id = 1, StartDate = new DateTime(2026, 1, 10), ExpirationDate = new DateTime(2027, 1, 10), Corporation = "Apple", ProductId = 1 }
    ];

    internal static List<IPhone> IPhones { get; } =
    [
        new() { Id = 1, PhoneId = 1, BatteryHealth = 98, ChargeCycle = 45 },
        new() { Id = 2, PhoneId = 3, BatteryHealth = 87, ChargeCycle = 612 }
    ];
}
