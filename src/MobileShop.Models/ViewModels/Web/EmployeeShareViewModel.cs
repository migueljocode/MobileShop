namespace MobileShop.Models.ViewModels.Web;

/// <summary>One active employee's share, as shown in the Reports share editor.</summary>
public sealed record EmployeeShareViewModel(int EmployeeId, string Name, int SharePercent);
