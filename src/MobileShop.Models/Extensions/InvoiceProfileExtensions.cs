namespace MobileShop.Models.Extensions;

public static class InvoiceProfileExtensions
{
    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this Phone phone)
    {
        yield return ("IMEI 1", phone.IMEI1);
        if (!string.IsNullOrWhiteSpace(phone.IMEI2))
            yield return ("IMEI 2", phone.IMEI2);
        if (!string.IsNullOrWhiteSpace(phone.Notes))
            yield return ("Notes", phone.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this AppleId appleId)
    {
        yield return ("Apple ID", appleId.Email);
        if (!string.IsNullOrWhiteSpace(appleId.Notes))
            yield return ("Notes", appleId.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this SecondHand secondHand)
    {
        var (years, months, days) = secondHand.GetUsedDurationBreakdown();
        yield return ("Used duration", $"{years} years, {months} months, {days} days");
        if (secondHand.TestPeriodDays.HasValue)
            yield return ("Test period", $"{secondHand.TestPeriodDays} days");
        if (!string.IsNullOrWhiteSpace(secondHand.Notes))
            yield return ("Notes", secondHand.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this Guarantee guarantee)
    {
        yield return ("Guarantee", $"{guarantee.Corporation} until {guarantee.ExpirationDate:d}");
        if (!string.IsNullOrWhiteSpace(guarantee.Notes))
            yield return ("Guarantee notes", guarantee.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this Laptop laptop)
    {
        yield return ("CPU", laptop.Cpu);
        yield return ("GPU", laptop.Gpu);
        yield return ("Display", $"{laptop.DisplaySize} inches");
        if (!string.IsNullOrWhiteSpace(laptop.Notes))
            yield return ("Notes", laptop.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this Cable cable)
    {
        yield return ("Connectors", $"{cable.Connector1} to {cable.Connector2}");
        yield return ("Length", $"{cable.Length} m");
        if (!string.IsNullOrWhiteSpace(cable.Notes))
            yield return ("Notes", cable.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this Charger charger)
    {
        yield return ("Wattage", $"{charger.Wattage} W");
        yield return ("Ports", charger.PortCount.ToString());
        yield return ("Power Delivery", charger.Pd ? "Yes" : "No");
        if (!string.IsNullOrWhiteSpace(charger.Notes))
            yield return ("Notes", charger.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this PowerBank powerBank)
    {
        yield return ("Capacity", $"{powerBank.CapacityMah} mAh");
        yield return ("Maximum wattage", $"{powerBank.MaxWattage} W");
        yield return ("Ports", powerBank.PortCount.ToString());
        yield return ("Power Delivery", powerBank.Pd ? "Yes" : "No");
        if (!string.IsNullOrWhiteSpace(powerBank.Notes))
            yield return ("Notes", powerBank.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this PortableStorage storage)
    {
        yield return ("Storage type", storage.Kind.ToString());
        yield return ("Capacity", $"{storage.StorageCapacityNavigation.Gb} GB");
        if (storage.Speed.HasValue)
            yield return ("Speed", $"{storage.Speed} MB/s");
        if (!string.IsNullOrWhiteSpace(storage.Notes))
            yield return ("Notes", storage.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this Case phoneCase)
    {
        if (!string.IsNullOrWhiteSpace(phoneCase.Notes))
            yield return ("Notes", phoneCase.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this Glass glass)
    {
        if (!string.IsNullOrWhiteSpace(glass.Notes))
            yield return ("Notes", glass.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this Tablet tablet)
    {
        if (!string.IsNullOrWhiteSpace(tablet.Notes))
            yield return ("Notes", tablet.Notes);
    }

    public static IEnumerable<(string Label, string Value)> GetInvoiceExtras(this SmartWatch smartWatch)
    {
        if (!string.IsNullOrWhiteSpace(smartWatch.Notes))
            yield return ("Notes", smartWatch.Notes);
    }
}
