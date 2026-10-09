using System.Text.RegularExpressions;
using MobileShop.Services.DataServices.Shared;

namespace MobileShop.Services.DataServices.Dal;

public partial class ProductsDataService
{
    public async Task<ServiceResult> UpdateProductAsync(EditProductInputModel input)
    {
        // The same includes as GetProductForEditAsync, tracked, so every profile the
        // type switch below needs is loaded and the changed entity can be saved
        // directly instead of re-attaching a half-loaded graph.
        var product = await products.FindTrackedWithIncludesAsync(
            input.ProductId,
            p => p.ModelNavigation,
            p => p.ModelNavigation.ManufacturerNavigation,
            p => p.ColorNavigation,
            p => p.SecondHandProfile,
            p => p.GuaranteeProfile,
            p => p.PhoneProfile,
            p => p.TabletProfile,
            p => p.SmartWatchProfile,
            p => p.LaptopProfile,
            p => p.AppleIdProfile,
            p => p.CableProfile,
            p => p.ChargerProfile,
            p => p.PowerBankProfile,
            p => p.PowerBankProfile.Ports,
            p => p.PortableStorageProfile,
            p => p.PortableStorageProfile.StorageCapacityNavigation,
            p => p.CaseProfile,
            p => p.CaseProfile.ModelFits,
            p => p.GlassProfile,
            p => p.GlassProfile.ModelFits);
        if (product is null)
            return new ServiceResult(false, "Product not found.", nameof(input.ProductId), null);

        if (!TryComputeFinishedPrice(input.Price, input.ProfitPercent, input.ProfitAmount, out var finishedPrice))
            return new ServiceResult(false, "The price is too large.", nameof(input.Price), null);

        // Update common fields
        product.ModelId = input.ModelId ?? product.ModelId;
        product.Price = finishedPrice;
        product.Barcode = input.Identifier;
        product.ColorId = input.ColorId > 0 ? input.ColorId : product.ColorId;

        // Update SecondHand profile
        if (input.IsSecondHand)
        {
            product.SecondHandProfile ??= new SecondHand();
            product.SecondHandProfile.TestPeriodDays = input.TestPeriodDays ?? 30;
            product.SecondHandProfile.UsedDurationDays = input.UsedDurationDays ?? 0;
            product.SecondHandProfile.Notes = NormalizeNote(input.SecondHandNotes);
        }
        else
        {
            product.SecondHandProfile = null;
        }

        // Update Guarantee profile
        if (!string.IsNullOrWhiteSpace(input.GuaranteeCorporation) || input.GuaranteeExpiry.HasValue)
        {
            product.GuaranteeProfile ??= new Guarantee();
            product.GuaranteeProfile.Corporation = input.GuaranteeCorporation ?? product.GuaranteeProfile.Corporation ?? "";
            if (input.GuaranteeExpiry.HasValue)
                product.GuaranteeProfile.ExpirationDate = input.GuaranteeExpiry.Value;
            product.GuaranteeProfile.Notes = NormalizeNote(input.GuaranteeNotes);
        }
        else
        {
            product.GuaranteeProfile = null;
        }

        switch (input.Type)
        {
            case "Phone":
                if (product.PhoneProfile is null) return new ServiceResult(false, "Product is not a phone.", nameof(input.Type), null);
                product.PhoneProfile.IMEI1 = string.IsNullOrWhiteSpace(input.IMEI1) ? product.PhoneProfile.IMEI1 : input.IMEI1.Trim();
                product.PhoneProfile.IMEI2 = string.IsNullOrWhiteSpace(input.IMEI2) ? null : input.IMEI2.Trim();
                product.PhoneProfile.PartNumberId = input.PartNumberId ?? product.PhoneProfile.PartNumberId;
                product.PhoneProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Tablet":
                if (product.TabletProfile is null) return new ServiceResult(false, "Product is not a tablet.", nameof(input.Type), null);
                product.TabletProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Smart Watch":
                if (product.SmartWatchProfile is null) return new ServiceResult(false, "Product is not a smart watch.", nameof(input.Type), null);
                product.SmartWatchProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Laptop":
                if (product.LaptopProfile is null) return new ServiceResult(false, "Product is not a laptop.", nameof(input.Type), null);
                if (!string.IsNullOrWhiteSpace(input.Cpu))
                {
                    var cpu = await cpus.FindAsync(c => c.Name == input.Cpu);
                    if (cpu is not null) product.LaptopProfile.CpuId = cpu.Id;
                }
                if (!string.IsNullOrWhiteSpace(input.Gpu))
                {
                    var gpu = await gpus.FindAsync(g => g.Name == input.Gpu);
                    if (gpu is not null) product.LaptopProfile.GpuId = gpu.Id;
                }
                product.LaptopProfile.DisplaySize = input.DisplaySize ?? product.LaptopProfile.DisplaySize;
                product.LaptopProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Apple ID":
                if (product.AppleIdProfile is null) return new ServiceResult(false, "Product is not an Apple ID.", nameof(input.Type), null);
                product.AppleIdProfile.Password = string.IsNullOrWhiteSpace(input.AppleIdPassword) ? product.AppleIdProfile.Password : input.AppleIdPassword.Trim();
                product.AppleIdProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Cable":
                if (product.CableProfile is null) return new ServiceResult(false, "Product is not a cable.", nameof(input.Type), null);
                product.CableProfile.Connector1 = input.Connector1 ?? product.CableProfile.Connector1;
                product.CableProfile.Connector2 = input.Connector2 ?? product.CableProfile.Connector2;
                product.CableProfile.Length = input.CableLength ?? product.CableProfile.Length;
                product.CableProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Charger":
                if (product.ChargerProfile is null) return new ServiceResult(false, "Product is not a charger.", nameof(input.Type), null);
                product.ChargerProfile.Wattage = input.Wattage ?? product.ChargerProfile.Wattage;
                product.ChargerProfile.Pd = input.Pd ?? product.ChargerProfile.Pd;
                product.ChargerProfile.PortCount = input.PortCount ?? product.ChargerProfile.PortCount;
                product.ChargerProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Power Bank":
                if (product.PowerBankProfile is null) return new ServiceResult(false, "Product is not a power bank.", nameof(input.Type), null);
                product.PowerBankProfile.CapacityMah = input.CapacityMah ?? product.PowerBankProfile.CapacityMah;
                product.PowerBankProfile.MaxWattage = input.MaxWattage ?? product.PowerBankProfile.MaxWattage;
                product.PowerBankProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Portable Storage":
                if (product.PortableStorageProfile is null) return new ServiceResult(false, "Product is not a portable storage.", nameof(input.Type), null);
                product.PortableStorageProfile.Kind = input.StorageKind ?? product.PortableStorageProfile.Kind;
                product.PortableStorageProfile.StorageCapacityId = input.StorageCapacityId ?? product.PortableStorageProfile.StorageCapacityId;
                product.PortableStorageProfile.Speed = input.Speed ?? product.PortableStorageProfile.Speed;
                product.PortableStorageProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Case":
                if (product.CaseProfile is null) return new ServiceResult(false, "Product is not a case.", nameof(input.Type), null);
                product.CaseProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Glass":
                if (product.GlassProfile is null) return new ServiceResult(false, "Product is not a glass.", nameof(input.Type), null);
                product.GlassProfile.Notes = NormalizeNote(input.Notes);
                break;

            default:
                return new ServiceResult(false, $"Unsupported product type: {input.Type}", nameof(input.Type), null);
        }

        // Zero rows is a successful no-op when the form was posted without real field changes.
        await products.SaveChangesAsync();
        return new ServiceResult(true, "Product updated successfully.", null, input.ProductId);
    }

    public async Task<Product?> GetProductForEditAsync(int id)
    {
#pragma warning disable CS8603, CS8602
        return await products.FindWithIncludesAsync(
            id,
            p => p.ModelNavigation,
            p => p.ModelNavigation.ManufacturerNavigation,
            p => p.ColorNavigation,
            p => p.SecondHandProfile,
            p => p.GuaranteeProfile,
            p => p.PhoneProfile,
            p => p.TabletProfile,
            p => p.SmartWatchProfile,
            p => p.LaptopProfile,
            p => p.AppleIdProfile,
            p => p.CableProfile,
            p => p.ChargerProfile,
            p => p.PowerBankProfile,
            p => p.PowerBankProfile.Ports,
            p => p.PortableStorageProfile,
            p => p.PortableStorageProfile.StorageCapacityNavigation,
            p => p.CaseProfile,
            p => p.CaseProfile.ModelFits,
            p => p.GlassProfile,
            p => p.GlassProfile.ModelFits);
#pragma warning restore CS8603, CS8602
    }

    private static bool TryComputeFinishedPrice(long paid, decimal? percent, long? amount, out long finished)
    {
        finished = 0;
        try
        {
            var value = amount.HasValue
                ? (decimal)paid + amount.Value
                : percent.HasValue
                    ? (decimal)paid + Math.Floor((decimal)paid * percent.Value / 100m)
                    : paid;
            if (value < 0) value = 0;
            if (value > MoneyLimits.MaxRials) return false;
            finished = (long)value;
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private static string? NormalizeNote(string? note)
        => string.IsNullOrWhiteSpace(note) ? null : note.Trim();
}
