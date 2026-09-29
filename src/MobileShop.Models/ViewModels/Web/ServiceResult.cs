namespace MobileShop.Models.ViewModels.Web;

/// <summary>Represents the outcome of a data-service operation.</summary>
/// <param name="Succeeded">Whether the operation succeeded.</param>
/// <param name="Message">The optional failure message.</param>
/// <param name="ErrorField">The model-field key the page should attach the error to, if any.</param>
/// <param name="EntityId">The identifier of the created entity, when the operation succeeds.</param>
public sealed record ServiceResult(bool Succeeded, string? Message, string? ErrorField = null, int? EntityId = null);
