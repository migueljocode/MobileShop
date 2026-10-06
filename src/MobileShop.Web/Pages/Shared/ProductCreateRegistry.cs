namespace MobileShop.Web.Pages.Shared;

public sealed record ProductCreateDefinition(string Key, string DisplayName, string PartialName, string PostHandler);

public static class ProductCreateRegistry
{
    private static readonly IReadOnlyDictionary<string, ProductCreateDefinition> Definitions =
        new Dictionary<string, ProductCreateDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["phone"] = new("phone", "Phone", "_ProductCreatePhoneForm", "CreatePhone"),
            ["appleid"] = new("appleid", "Apple ID", "_ProductCreateAppleIdForm", "CreateAppleId"),
            ["glass"] = new("glass", "Glass", "_ProductCreateGlassForm", "CreateGlass"),
            ["tablet"] = new("tablet", "Tablet", "_ProductCreateTabletForm", "CreateTablet"),
            ["smartwatch"] = new("smartwatch", "Smart Watch", "_ProductCreateSmartWatchForm", "CreateSmartWatch"),
            ["laptop"] = new("laptop", "Laptop", "_ProductCreateLaptopForm", "CreateLaptop"),
            ["cable"] = new("cable", "Cable", "_ProductCreateCableForm", "CreateCable"),
            ["charger"] = new("charger", "Charger", "_ProductCreateChargerForm", "CreateCharger"),
            ["powerbank"] = new("powerbank", "Power Bank", "_ProductCreatePowerBankForm", "CreatePowerBank"),
            ["portablestorage"] = new("portablestorage", "Portable Storage", "_ProductCreatePortableStorageForm", "CreatePortableStorage"),
            ["case"] = new("case", "Case", "_ProductCreateCaseForm", "CreateCase"),
            ["cable"] = new("cable", "Cable", "_ProductCreateCableForm", "CreateCable"),
            ["charger"] = new("charger", "Charger", "_ProductCreateChargerForm", "CreateCharger"),
            ["powerbank"] = new("powerbank", "Power Bank", "_ProductCreatePowerBankForm", "CreatePowerBank"),
            ["portablestorage"] = new("portablestorage", "Portable Storage", "_ProductCreatePortableStorageForm", "CreatePortableStorage"),
            ["case"] = new("case", "Case", "_ProductCreateCaseForm", "CreateCase"),
        };

    public static bool TryGet(string? key, out ProductCreateDefinition definition)
        => Definitions.TryGetValue(key?.Trim() ?? string.Empty, out definition!);
}