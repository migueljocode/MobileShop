namespace MobileShop.Web.Pages.Products;

public class EditModel(
    IProductsDataService dataService) : PageModel
{
    [BindProperty] public EditProductInputModel Product { get; set; } = new();
    public IReadOnlyList<DropdownOptionViewModel> Colors { get; private set; } = [];
    public IReadOnlyList<DropdownOptionViewModel> PartNumbers { get; private set; } = [];
    public IReadOnlyList<string> Corporations { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, string? type = null)
    {
        var product = await dataService.GetProductForEditAsync(id);
        if (product is null) return NotFound();

        // Use actual type from product; type param is optional for routing only
        var actualType = GetProductType(product);
        
        // If type provided, validate it matches (with normalization)
        if (!string.IsNullOrEmpty(type))
        {
            var normalizedType = NormalizeType(type);
            var normalizedActualType = NormalizeType(actualType);
            if (!string.Equals(normalizedType, normalizedActualType, StringComparison.OrdinalIgnoreCase))
            {
                // Don't 404 - just use actual type, the ID is the real identifier
                // This handles URL type mismatches gracefully
            }
        }

        Product = MapToEditModel(product, actualType);
        await PopulateDropdownsAsync(actualType);
        return Page();
    }

    private static string NormalizeType(string type)
        => type.Replace(" ", "").Replace("-", "").ToLowerInvariant();

    public async Task<IActionResult> OnGetPartNumbersAsync(int modelId)
    {
        if (modelId <= 0)
            return new JsonResult(Array.Empty<object>());

        var options = await dataService.GetPartNumbersAsync(modelId);
        return new JsonResult(options.Select(o => new { o.Id, o.Name }));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Empty optional IMEI fields bind as "" and would fail a strict 15-digit regex.
        if (string.IsNullOrWhiteSpace(Product.IMEI1))
            Product.IMEI1 = null;
        if (string.IsNullOrWhiteSpace(Product.IMEI2))
            Product.IMEI2 = null;
        ModelState.Remove("Product.IMEI1");
        ModelState.Remove("Product.IMEI2");
        if (string.Equals(Product.Type, "Phone", StringComparison.OrdinalIgnoreCase))
        {
            if (Product.IMEI1 is null || Product.IMEI1.Length != 15)
                ModelState.AddModelError("Product.IMEI1", "IMEI must be exactly 15 digits.");
            if (Product.IMEI2 is not null && Product.IMEI2.Length != 15)
                ModelState.AddModelError("Product.IMEI2", "IMEI must be exactly 15 digits.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(Product.Type);
            return Page();
        }

        var result = await dataService.UpdateProductAsync(Product);
        if (!result.Succeeded)
        {
            if (result.ErrorField is not null)
                ModelState.AddModelError(result.ErrorField, result.Message!);
            else
                ModelState.AddModelError(string.Empty, result.Message!);
            await PopulateDropdownsAsync(Product.Type);
            return Page();
        }

        TempData["SuccessMessage"] = "Product updated successfully.";
        return RedirectToPage("/Products/Details", new { id = Product.ProductId, type = Product.Type });
    }

    private async Task PopulateDropdownsAsync(string type)
    {
        if (type == "Phone" || type == "Tablet" || type == "Smart Watch" || type == "Laptop")
        {
            Colors = await dataService.GetColorsAsync();
        }

        if (type == "Phone" && Product.ModelId > 0)
        {
            PartNumbers = await dataService.GetPartNumbersAsync(Product.ModelId);
        }

        if (type == "Phone" || type == "Tablet" || type == "Smart Watch" || type == "Laptop")
        {
            Corporations = await dataService.GetGuaranteeCorporationsAsync();
        }
    }

    private static EditProductInputModel MapToEditModel(Product product, string type)
    {
        return new EditProductInputModel
        {
            ProductId = product.Id,
            Type = type,
            Manufacturer = product.ModelNavigation.ManufacturerNavigation.Name,
            Model = product.ModelNavigation.Name,
            Identifier = product.Barcode,
            Color = product.ColorNavigation?.Name,
            ColorId = product.ColorId ?? 0,
            Price = product.Price,
            ProfitPercent = 0, // Not stored directly, would need calculation
            ProfitAmount = 0,
            IsSecondHand = product.SecondHandProfile != null,
            GuaranteeCorporation = product.GuaranteeProfile?.Corporation,
            GuaranteeExpiry = product.GuaranteeProfile?.ExpirationDate,
            GuaranteeNotes = product.GuaranteeProfile?.Notes,
            Notes = GetProfileNotes(product, type),
            // Phone-specific
            IMEI1 = product.PhoneProfile?.IMEI1,
            IMEI2 = product.PhoneProfile?.IMEI2,
            PartNumberId = product.PhoneProfile?.PartNumberId,
            ModelId = product.ModelId,
            SupportsDualSim = product.PhoneProfile?.SupportsDualSim ?? false,
            SupportsEsim = product.PhoneProfile?.SupportsEsim ?? false,
            // Laptop-specific
            Cpu = product.LaptopProfile?.CpuNavigation?.Name,
            Gpu = product.LaptopProfile?.GpuNavigation?.Name,
            DisplaySize = product.LaptopProfile?.DisplaySize,
            // Apple ID-specific
            AppleIdPassword = product.AppleIdProfile?.Password,
            // Cable-specific
            Connector1 = product.CableProfile?.Connector1,
            Connector2 = product.CableProfile?.Connector2,
            CableLength = product.CableProfile?.Length,
            // Charger-specific
            Wattage = product.ChargerProfile?.Wattage,
            Pd = product.ChargerProfile?.Pd,
            PortCount = product.ChargerProfile?.PortCount,
            // Power Bank-specific
            CapacityMah = product.PowerBankProfile?.CapacityMah,
            MaxWattage = product.PowerBankProfile?.MaxWattage,
            PortTypes = product.PowerBankProfile?.Ports?.Select(p => p.Connector.ToString()).ToList() ?? [],
            // Portable Storage-specific
            StorageKind = product.PortableStorageProfile?.Kind,
            StorageCapacityId = product.PortableStorageProfile?.StorageCapacityId,
            StorageCapacityLabel = product.PortableStorageProfile?.StorageCapacityNavigation?.Gb + " GB",
            Speed = product.PortableStorageProfile?.Speed,
            // Case/Glass-specific
            CompatibleModels = product.CaseProfile?.ModelFits?.Select(f => f.ModelNavigation.Name).ToList() ??
                               product.GlassProfile?.ModelFits?.Select(f => f.ModelNavigation.Name).ToList() ?? [],
            TestPeriodDays = product.SecondHandProfile?.TestPeriodDays,
            UsedDurationDays = product.SecondHandProfile?.UsedDurationDays,
            SecondHandNotes = product.SecondHandProfile?.Notes
        };
    }

    private static string GetProductType(Product product)
    {
        if (product.PhoneProfile != null) return "Phone";
        if (product.AppleIdProfile != null) return "Apple ID";
        if (product.TabletProfile != null) return "Tablet";
        if (product.SmartWatchProfile != null) return "Smart Watch";
        if (product.LaptopProfile != null) return "Laptop";
        if (product.CableProfile != null) return "Cable";
        if (product.ChargerProfile != null) return "Charger";
        if (product.PowerBankProfile != null) return "Power Bank";
        if (product.PortableStorageProfile != null) return "Portable Storage";
        if (product.CaseProfile != null) return "Case";
        if (product.GlassProfile != null) return "Glass";
        return "Unknown";
    }

    private static string? GetProfileNotes(Product product, string type)
    {
        return type switch
        {
            "Phone" => product.PhoneProfile?.Notes,
            "Tablet" => product.TabletProfile?.Notes,
            "Smart Watch" => product.SmartWatchProfile?.Notes,
            "Laptop" => product.LaptopProfile?.Notes,
            "Apple ID" => product.AppleIdProfile?.Notes,
            "Cable" => product.CableProfile?.Notes,
            "Charger" => product.ChargerProfile?.Notes,
            "Power Bank" => product.PowerBankProfile?.Notes,
            "Portable Storage" => product.PortableStorageProfile?.Notes,
            "Case" => product.CaseProfile?.Notes,
            "Glass" => product.GlassProfile?.Notes,
            _ => null
        };
    }
}
