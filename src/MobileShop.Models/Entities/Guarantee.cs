namespace MobileShop.Models.Entities;

[Table("Guarantees")]
public class Guarantee : BaseEntity
{
    [Range(typeof(DateTime), "2000-01-01", "2100-01-01", ErrorMessage = "{0} must be between {1} and {2}.")]
    public DateTime StartDate { get; set; }

    [Range(typeof(DateTime), "2000-01-01", "2100-01-01", ErrorMessage = "{0} must be between {1} and {2}.")]
    public DateTime ExpirationDate { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Corporation { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;
}
