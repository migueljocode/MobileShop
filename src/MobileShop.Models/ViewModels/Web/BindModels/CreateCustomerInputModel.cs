namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when creating a customer.</summary>
public sealed class CreateCustomerInputModel
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string NationalId { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }
}
