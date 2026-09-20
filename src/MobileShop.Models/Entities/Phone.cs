namespace MobileShop.Models.Entities;

[Table("Phones")]
public class Phone : BaseEntity
{
    // [0-9] instead of \d - \d matches any Unicode digit (Persian/Arabic-Indic included), [0-9] is ASCII-only,
    // and the regex already bounds the length to 15, so no separate StringLength needed
    [Required(ErrorMessage = "{0} is required.")]
    [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "{0} must be exactly 15 digits.")]
    public string IMEI1 { get; set; } = string.Empty;

    [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "{0} must be exactly 15 digits.")]
    public string? IMEI2 { get; set; }

    public bool OwnershipTransferred { get; set; }

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }

    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;
}
