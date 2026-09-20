namespace MobileShop.Models.Entities;

[Table("Cables")]
public class Cable : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    public CableConnector Connector1 { get; set; }

    public CableConnector Connector2 { get; set; }

    /// <summary>Cable length in meters.</summary>
    [Range(0, double.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    [Column(TypeName = "decimal(4,2)")]
    public decimal Length { get; set; }

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }
}