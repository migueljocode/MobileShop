namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when creating a seller.</summary>
public sealed class CreateSellerInputModel
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
    public SellerEntityType EntityType { get; set; } = SellerEntityType.Real;

    [StringLength(500)]
    public string? Notes { get; set; }
}
