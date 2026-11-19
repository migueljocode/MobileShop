namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when updating the shop profile.</summary>
public sealed class ShopProfileInput
{
    [Required(ErrorMessage = "Shop name is required.")]
    [StringLength(100, ErrorMessage = "Shop name cannot exceed {1} characters.")]
    [Display(Name = "Shop name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Address cannot exceed {1} characters.")]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Phone is required.")]
    [StringLength(30, ErrorMessage = "Phone cannot exceed {1} characters.")]
    [Display(Name = "Phone")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Instagram cannot exceed {1} characters.")]
    [Display(Name = "Instagram")]
    public string? Instagram { get; set; }
}
