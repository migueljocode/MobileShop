namespace MobileShop.Models.ViewModels.Web;

/// <summary>Represents the outcome of a dropdown create modal that posts an inline form.</summary>
/// <param name="Succeeded">Whether the create succeeded.</param>
/// <param name="Option">The dropdown option to bind on success, when the create succeeded.</param>
/// <param name="Error">The user-facing error message, when the create failed.</param>
/// <param name="StatusCode">The HTTP status code the page must return so the existing JS keeps behaving.</param>
public sealed record DropdownCreateResult(bool Succeeded, DropdownOptionViewModel? Option, string? Error, int StatusCode);
