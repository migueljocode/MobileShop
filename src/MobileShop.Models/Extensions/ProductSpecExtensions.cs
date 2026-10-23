namespace MobileShop.Models.Extensions;

/// <summary>
/// Describes what makes two products the same kind of item for a factor: model, colour and the
/// specifications that differ between units. Barcode, IMEI, notes and warranty data are deliberately
/// excluded, so two units with identical specs share one key.
/// </summary>
public static class ProductSpecExtensions
{
    private const string KeySeparator = "|";
    private const string LabelSeparator = " · ";

    /// <summary>Stable key of the product's specs. Equal keys mean the products are interchangeable.</summary>
    public static string ToSpecKey(this Product product)
    {
        var segments = new List<string>
        {
            product.ModelId.ToString(),
            product.ColorId?.ToString() ?? string.Empty,
        };

        segments.AddRange(DeviceSpecSegments(product.DeviceSpecProfile));
        segments.AddRange(PhoneSegments(product.PhoneProfile));
        segments.AddRange(LaptopSegments(product.LaptopProfile));
        segments.AddRange(CableSegments(product.CableProfile));
        segments.AddRange(ChargerSegments(product.ChargerProfile));
        segments.AddRange(PowerBankSegments(product.PowerBankProfile));
        segments.AddRange(PortableStorageSegments(product.PortableStorageProfile));
        segments.Add(FitSegment(product.GlassProfile?.ModelFits.Select(fit => fit.ModelId)));
        segments.Add(FitSegment(product.CaseProfile?.ModelFits.Select(fit => fit.ModelId)));
        segments.Add(product.SecondHandProfile is null ? string.Empty : "second-hand");

        return string.Join(KeySeparator, segments);
    }

    /// <summary>Short human text naming the product's colour and built-in storage, empty when neither is known.</summary>
    public static string ToSpecLabel(this Product product)
    {
        var parts = new List<string>();

        if (product.ColorNavigation is not null)
            parts.Add(product.ColorNavigation.Name);

        var storage = product.DeviceSpecProfile?.StorageCapacityNavigation;
        if (storage is not null)
            parts.Add($"{storage.Gb} GB");

        return string.Join(LabelSeparator, parts);
    }

    private static IEnumerable<string> DeviceSpecSegments(DeviceSpec? spec) =>
        spec is null
            ? [string.Empty, string.Empty, string.Empty]
            : [spec.Ram.ToString(), spec.StorageCapacityId.ToString(), spec.ChargeSpeed.ToString()];

    private static IEnumerable<string> PhoneSegments(Phone? phone) =>
        phone is null
            ? [string.Empty, string.Empty, string.Empty]
            : [
                phone.PartNumberId?.ToString() ?? string.Empty,
                phone.SupportsDualSim?.ToString() ?? string.Empty,
                phone.SupportsEsim?.ToString() ?? string.Empty,
            ];

    private static IEnumerable<string> LaptopSegments(Laptop? laptop) =>
        laptop is null
            ? [string.Empty, string.Empty, string.Empty]
            : [
                laptop.CpuId.ToString(),
                laptop.GpuId?.ToString() ?? string.Empty,
                laptop.DisplaySize.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ];

    private static IEnumerable<string> CableSegments(Cable? cable) =>
        cable is null
            ? [string.Empty, string.Empty, string.Empty]
            : [
                cable.Connector1.ToString(),
                cable.Connector2.ToString(),
                cable.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ];

    private static IEnumerable<string> ChargerSegments(Charger? charger) =>
        charger is null
            ? [string.Empty, string.Empty, string.Empty]
            : [charger.Wattage.ToString(), charger.PortCount.ToString(), charger.Pd.ToString()];

    private static IEnumerable<string> PowerBankSegments(PowerBank? powerBank) =>
        powerBank is null
            ? [string.Empty, string.Empty, string.Empty, string.Empty]
            : [
                powerBank.CapacityMah.ToString(),
                powerBank.MaxWattage.ToString(),
                powerBank.PortCount.ToString(),
                powerBank.Pd.ToString(),
            ];

    private static IEnumerable<string> PortableStorageSegments(PortableStorage? storage) =>
        storage is null
            ? [string.Empty, string.Empty, string.Empty]
            : [
                storage.Kind.ToString(),
                storage.StorageCapacityId.ToString(),
                storage.Speed?.ToString() ?? string.Empty,
            ];

    private static string FitSegment(IEnumerable<int>? modelIds) =>
        modelIds is null ? string.Empty : string.Join(",", modelIds.Order());
}
