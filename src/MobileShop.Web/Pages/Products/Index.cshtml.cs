namespace MobileShop.Web.Pages.Products;

public class IndexModel(
    IProductsDataService dataService) : PageModel
{
    private const string DefaultSortBy = "name";

    public string Type { get; private set; } = "all";
    public string Availability { get; private set; } = "all";
    public string SortBy { get; private set; } = DefaultSortBy;
    public string SortDirection { get; private set; } = "asc";
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    /// <summary>The selected part-number filter, or null when no part number is selected.</summary>
    public int? PartNumberId { get; private set; }

    /// <summary>The selected manufacturer filter, or null when no manufacturer is selected.</summary>
    public int? ManufacturerId { get; private set; }

    /// <summary>The selected model filter, or null when no model is selected.</summary>
    public int? ModelId { get; private set; }

    /// <summary>
    /// The part-number dropdown options, with a leading "All" entry. Only built for the Phones
    /// view, because no other view can be narrowed by a part number.
    /// </summary>
    public IReadOnlyList<SelectListItem> PartNumberOptions { get; private set; } = [];

    /// <summary>All manufacturers with a leading "All" option.</summary>
    public IReadOnlyList<SelectListItem> ManufacturerOptions { get; private set; } = [];

    /// <summary>The models for the selected manufacturer, with a leading "All" option.</summary>
    public IReadOnlyList<SelectListItem> ModelOptions { get; private set; } = [];

    /// <summary>Whether the part-number filter control applies to the current view (phones only).</summary>
    public bool ShowPartNumberFilter => Type == "phone";

    /// <summary>Whether the model filter is active based on the selected manufacturer.</summary>
    public bool ShowModelFilter => ManufacturerId is > 0;

    /// <summary>The route value for preserving the selected availability when switching product type.</summary>
    public string? AvailabilityRoute => Availability == "all" ? null : Availability;

    public string NameColumnLabel => Type == "appleid" ? "Customer" : "Product";

    public async Task OnGetAsync(
        string? type = null,
        int? partNumberId = null,
        string? availability = null,
        int? manufacturerId = null,
        int? modelId = null,
        string? sortBy = null,
        string? sortDirection = null)
    {
        Type = string.IsNullOrWhiteSpace(type) ? "all" : type.ToLowerInvariant();

        var normalizedAvailability = availability?.Trim().ToLowerInvariant();
        Availability = normalizedAvailability is "available" or "sold" ? normalizedAvailability : "all";
        SortBy = NormalizeSortBy(sortBy);
        SortDirection = sortDirection?.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase) == true ? "desc" : "asc";

        // Zero/negative ids are treated as no selection so a stray query value cannot blank the list.
        PartNumberId = partNumberId is > 0 ? partNumberId : null;
        ManufacturerId = manufacturerId is > 0 ? manufacturerId : null;
        ModelId = modelId is > 0 ? modelId : null;

        var manufacturerOptions = await dataService.GetManufacturersAsync();
        ManufacturerOptions = manufacturerOptions
            .Select(option => new SelectListItem(option.Name, option.Id.ToString()))
            .Prepend(new SelectListItem("All manufacturers", string.Empty))
            .ToList()
            .AsReadOnly();

        if (ManufacturerId is not null && manufacturerOptions.All(option => option.Id != ManufacturerId))
            ManufacturerId = null;

        IReadOnlyList<DropdownOptionViewModel> manufacturerModels = [];
        if (ManufacturerId is not null)
        {
            manufacturerModels = await dataService.GetModelsAsync(ManufacturerId.Value, GetCategoryName());
            if (ModelId is not null && manufacturerModels.All(option => option.Id != ModelId))
                ModelId = null;

            ModelOptions = manufacturerModels
                .Select(option => new SelectListItem(option.Name, option.Id.ToString()))
                .Prepend(new SelectListItem("All models", string.Empty))
                .ToList()
                .AsReadOnly();
        }
        else
        {
            ModelId = null;
            ModelOptions = [];
        }

        if (ShowPartNumberFilter)
        {
            var options = await dataService.GetInventoryPartNumbersAsync();

            // A stale id (the phone carrying it was removed) must not leave an active filter pointing
            // at an option the dropdown can no longer show.
            if (PartNumberId is not null && options.All(option => option.Id != PartNumberId))
                PartNumberId = null;

            PartNumberOptions = options
                .Select(option => new SelectListItem(option.Name, option.Id.ToString()))
                .Prepend(new SelectListItem("All part numbers", string.Empty))
                .ToList()
                .AsReadOnly();
        }

        Products = await dataService.GetInventoryRowsAsync(
            Type,
            PartNumberId,
            Availability == "all" ? null : Availability);

        var manufacturerName = ManufacturerId is null
            ? null
            : manufacturerOptions.FirstOrDefault(option => option.Id == ManufacturerId)?.Name;
        var modelName = ModelId is null
            ? null
            : manufacturerModels.FirstOrDefault(option => option.Id == ModelId)?.Name;

        var filtered = Products.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(manufacturerName))
            filtered = filtered.Where(row => row.Name.StartsWith(manufacturerName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(modelName))
            filtered = filtered.Where(row => row.Name.Contains(modelName, StringComparison.OrdinalIgnoreCase));

        Products = ApplySorting(filtered).ToList().AsReadOnly();
    }

    private string NormalizeSortBy(string? sortBy)
    {
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "type" => "type",
            "identifier" => "identifier",
            "color" => "color",
            "status" => "status",
            "name" => "name",
            _ => DefaultSortBy,
        };
    }

    private string GetCategoryName()
    {
        return Type switch
        {
            "phone" => "Phone",
            "appleid" => "Phone",
            "glass" => "Phone",
            "tablet" => "Tablet",
            "smartwatch" => "Smart Watch",
            "laptop" => "Laptop",
            "cable" => "Cable",
            "charger" => "Charger",
            "powerbank" => "Power Bank",
            "portablestorage" => "Portable Storage",
            "case" => "Case",
            _ => "Phone",
        };
    }

    private IEnumerable<ProductListItemViewModel> ApplySorting(IEnumerable<ProductListItemViewModel> rows)
    {
        var isDescending = SortDirection == "desc";

        return SortBy switch
        {
            "type" => isDescending
                ? rows.OrderByDescending(row => row.Type).ThenByDescending(row => row.Name)
                : rows.OrderBy(row => row.Type).ThenBy(row => row.Name),
            "identifier" => isDescending
                ? rows.OrderByDescending(row => row.Identifier).ThenByDescending(row => row.Name)
                : rows.OrderBy(row => row.Identifier).ThenBy(row => row.Name),
            "color" => isDescending
                ? rows.OrderByDescending(row => row.Color ?? string.Empty).ThenByDescending(row => row.Name)
                : rows.OrderBy(row => row.Color ?? string.Empty).ThenBy(row => row.Name),
            "status" => isDescending
                ? rows.OrderByDescending(row => row.IsSold).ThenByDescending(row => row.Name)
                : rows.OrderBy(row => row.IsSold).ThenBy(row => row.Name),
            _ => isDescending
                ? rows.OrderByDescending(row => row.Name).ThenByDescending(row => row.EntityId)
                : rows.OrderBy(row => row.Name).ThenBy(row => row.EntityId),
        };
    }
}
