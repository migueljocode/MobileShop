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

    /// <summary>Answer to security question 1; shown on the Apple ID delivery factor.</summary>
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? SecurityAnswer1 { get; set; }

    /// <summary>Answer to security question 2; shown on the Apple ID delivery factor.</summary>
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? SecurityAnswer2 { get; set; }

    /// <summary>Answer to security question 3; shown on the Apple ID delivery factor.</summary>
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? SecurityAnswer3 { get; set; }

    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;
}
