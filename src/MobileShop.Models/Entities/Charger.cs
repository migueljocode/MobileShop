namespace MobileShop.Models.Entities;

[Table("Chargers")]
public class Charger : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    /// <summary>Output wattage in watts (W).</summary>
    [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public int Wattage { get; set; }

    /// <summary>Whether the charger supports USB Power Delivery (PD).</summary>
    public bool Pd { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "{0} must be at least {1}.")]
    public int PortCount { get; set; }

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }
}