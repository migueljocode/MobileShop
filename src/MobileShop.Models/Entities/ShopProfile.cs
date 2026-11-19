namespace MobileShop.Models.Entities;

/// <summary>
/// Single-row shop identity stored in the database and printed on generated invoices.
/// </summary>
[Table("ShopProfiles")]
public class ShopProfile : BaseEntity
{
    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(30, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Instagram { get; set; } = string.Empty;
}
