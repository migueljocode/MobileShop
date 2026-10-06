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
        };

    public static bool TryGet(string? key, out ProductCreateDefinition definition)
        => Definitions.TryGetValue(key?.Trim() ?? string.Empty, out definition!);
}