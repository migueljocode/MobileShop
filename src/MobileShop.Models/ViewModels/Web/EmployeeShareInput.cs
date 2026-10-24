namespace MobileShop.Models.ViewModels.Web;

/// <summary>A posted share for one active employee.</summary>
public class EmployeeShareInput
{
    public int EmployeeId { get; set; }

    [Range(0, 100, ErrorMessage = "Share percent must be between 0 and 100.")]
    public int SharePercent { get; set; }
}
