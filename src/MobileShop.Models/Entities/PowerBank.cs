namespace MobileShop.Models.Entities;

[Table("PowerBanks")]
public class PowerBank : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    /// <summary>Battery capacity in milliampere-hours (mAh).</summary>
    [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public int CapacityMah { get; set; }

    /// <summary>Maximum output wattage in watts (W).</summary>
    [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public int MaxWattage { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "{0} must be at least {1}.")]
    public int PortCount { get; set; }

    /// <summary>Whether the power bank supports USB Power Delivery (PD).</summary>
    public bool Pd { get; set; }

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }
}