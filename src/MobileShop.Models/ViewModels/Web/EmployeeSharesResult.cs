namespace MobileShop.Models.ViewModels.Web;

/// <summary>The outcome of saving employee shares.</summary>
public sealed record EmployeeSharesResult(bool Success, string? Error)
{
    /// <summary>A successful save.</summary>
    public static EmployeeSharesResult Ok { get; } = new(true, null);

    /// <summary>A rejected save, carrying the reason to show the user.</summary>
    public static EmployeeSharesResult Fail(string error) => new(false, error);
}
