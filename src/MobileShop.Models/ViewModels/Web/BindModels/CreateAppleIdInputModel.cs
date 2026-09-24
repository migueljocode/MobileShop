namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when creating an Apple ID product.</summary>
public sealed class CreateAppleIdInputModel
{
    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }
}
