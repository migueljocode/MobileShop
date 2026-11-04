namespace MobileShop.Models.ViewModels.Web;

/// <summary>
/// Data required to render the customer-facing Apple ID delivery form for one sold Apple ID.
/// Optional answers and notes are left blank on the printed form when absent.
/// </summary>
/// <param name="CustomerName">The customer receiving the Apple ID.</param>
/// <param name="CustomerPhone">The customer's contact number, when known.</param>
/// <param name="Date">The delivery (sale) date.</param>
/// <param name="Email">The Apple ID email address.</param>
/// <param name="Password">The Apple ID password.</param>
/// <param name="SecurityAnswer1">The answer to security question 1.</param>
/// <param name="SecurityAnswer2">The answer to security question 2.</param>
/// <param name="SecurityAnswer3">The answer to security question 3.</param>
/// <param name="Notes">Free-form notes entered for the Apple ID.</param>
public sealed record AppleIdFactorViewModel(
    string CustomerName,
    string? CustomerPhone,
    DateTime Date,
    string Email,
    string Password,
    string? SecurityAnswer1,
    string? SecurityAnswer2,
    string? SecurityAnswer3,
    string? Notes);
