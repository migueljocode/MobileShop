namespace MobileShop.Models.ViewModels.Web;

/// <summary>Represents an option rendered by a dropdown control.</summary>
/// <param name="Id">The option identifier.</param>
/// <param name="Name">The option display text.</param>
public sealed record DropdownOptionViewModel(int Id, string Name);
