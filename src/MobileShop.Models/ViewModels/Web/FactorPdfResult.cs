namespace MobileShop.Models.ViewModels.Web;

/// <summary>Represents the outcome of generating a list factor PDF.</summary>
/// <param name="Succeeded">Whether a factor was produced.</param>
/// <param name="Bytes">The generated PDF bytes when <paramref name="Succeeded"/> is <c>true</c>; otherwise <c>null</c>.</param>
/// <param name="Error">The user-facing failure message when <paramref name="Succeeded"/> is <c>false</c>; otherwise <c>null</c>.</param>
public sealed record FactorPdfResult(bool Succeeded, byte[]? Bytes, string? Error);
