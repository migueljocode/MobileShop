namespace MobileShop.Web.Pages.Products;

public class IndexModel(
    IProductsDataService dataService) : PageModel
{
    public string Type { get; private set; } = "all";
    public string Availability { get; private set; } = "all";
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    /// <summary>The selected part-number filter, or null when no part number is selected.</summary>
    public int? PartNumberId { get; private set; }

    /// <summary>
    /// The part-number dropdown options, with a leading "All" entry. Only built for the Phones
    /// view, because no other view can be narrowed by a part number.
    /// </summary>
    public IReadOnlyList<SelectListItem> PartNumberOptions { get; private set; } = [];

    /// <summary>Whether the part-number filter control applies to the current view (phones only).</summary>
    public bool ShowPartNumberFilter => Type == "phone";

    /// <summary>The route value for preserving the selected availability when switching product type.</summary>
    public string? AvailabilityRoute => Availability == "all" ? null : Availability;

    public async Task OnGetAsync(string? type = null, int? partNumberId = null, string? availability = null)
    {
        Type = string.IsNullOrWhiteSpace(type) ? "all" : type.ToLowerInvariant();

        var normalizedAvailability = availability?.Trim().ToLowerInvariant();
        Availability = normalizedAvailability is "available" or "sold" ? normalizedAvailability : "all";

        // Zero/negative ids are treated as no selection so a stray query value cannot blank the list.
        PartNumberId = partNumberId is > 0 ? partNumberId : null;

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
    }
}
