namespace MobileShop.Models.Entities;

[Table("AppleIds")]
public class AppleId : BaseEntity
{
    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    [EmailAddress(ErrorMessage = "{0} is not a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Password { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }

    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;
}
