namespace MobileShop.Models.ViewModels.Web;

/// <summary>
/// Carries the selectable product rows a shared transaction product-picker options partial
/// renders. The hosting page keeps its own tag-helper select and price controls (so client and
/// server validation, the selected value and the posted price survive round-trips); the partial
/// renders only the product <c>option</c> elements plus the suggested-price display.
/// </summary>
public sealed record ProductPickerViewModel(
    IReadOnlyList<ProductListItemViewModel> Products,
    int SelectedProductId = 0);
