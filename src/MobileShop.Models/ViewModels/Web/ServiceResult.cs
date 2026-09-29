namespace MobileShop.Models.ViewModels.Web;

/// <summary>Represents the outcome of a data-service operation.</summary>
/// <param name="Succeeded">Whether the operation succeeded.</param>
/// <param name="Message">The optional failure message.</param>
public sealed record ServiceResult(bool Succeeded, string? Message);
