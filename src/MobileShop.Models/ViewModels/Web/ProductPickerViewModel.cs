namespace MobileShop.Models.ViewModels.Web;

/// <summary>
/// Carries the data a shared transaction product picker needs: the selectable product rows and
/// the HTML field prefix (<c>Input</c>) the hosting page binds under.
/// </summary>
public sealed record ProductPickerViewModel(
    IReadOnlyList<ProductListItemViewModel> Products,
    string FieldPrefix);
