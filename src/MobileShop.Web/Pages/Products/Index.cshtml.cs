using Microsoft.AspNetCore.Mvc.Rendering;

namespace MobileShop.Web.Pages.Products;

public class IndexModel(
    IProductsDataService dataService) : PageModel
{
    public string Type { get; private set; } = "all";
    public IReadOnlyList<ProductListItemViewModel> Products { get; private set; } = [];

    /// <summary>The selected part-number filter, or null when no part number is selected.</summary>
    public int? PartNumberId { get; private set; }

    /// <summary>The part-number dropdown options for the filter, with a leading "All" entry.</summary>
    public IReadOnlyList<SelectListItem> PartNumberOptions { get; private set; } = [];

    public async Task OnGetAsync(string? type = null, int? partNumberId = null)
    {
        Type = string.IsNullOrWhiteSpace(type) ? "all" : type.ToLowerInvariant();

        // Zero/negative ids are treated as no selection so a stray query value cannot blank the list.
        PartNumberId = partNumberId is > 0 ? partNumberId : null;

        PartNumberOptions = (await dataService.GetPartNumbersAsync())
            .Select(option => new SelectListItem(option.Name, option.Id.ToString()))
            .Prepend(new SelectListItem("All part numbers", string.Empty))
            .ToList()
            .AsReadOnly();

        Products = await dataService.GetInventoryRowsAsync(Type, PartNumberId);
    }
}
