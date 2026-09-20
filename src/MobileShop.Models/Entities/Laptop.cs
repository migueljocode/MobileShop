namespace MobileShop.Models.Entities;

[Table("Laptops")]
public class Laptop : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Cpu { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Gpu { get; set; } = string.Empty;

    /// <summary>Screen size in inches.</summary>
    [Range(0, double.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    [Column(TypeName = "decimal(4,1)")]
    public decimal DisplaySize { get; set; }

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }
}