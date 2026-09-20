namespace MobileShop.Models.Entities;

/// <summary>
/// Battery and hardware info for an Apple device product. One-to-one with <see cref="Product"/>,
/// and intentionally not specific to phones - it replaces the old <c>IPhone</c> entity.
/// </summary>
[Table("AppleInfos")]
public class AppleInfo : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    /// <summary>Battery health as a percentage (0-100).</summary>
    [Range(0, 100, ErrorMessage = "{0} must be between {1} and {2}.")]
    public int BatteryHealth { get; set; }

    /// <summary>Number of completed battery charge cycles.</summary>
    [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public int ChargeCycle { get; set; }

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }
}