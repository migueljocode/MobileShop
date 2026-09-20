namespace MobileShop.Models.Entities;

/// <summary>
/// Hardware specification of a device product (phone, tablet, laptop, ...). One-to-one with
/// <see cref="Product"/>.
/// </summary>
/// <remarks>
/// <see cref="StorageCapacityId"/> is the <b>built-in</b> storage of the device itself.
/// It is deliberately a different relationship from <see cref="PortableStorage.StorageCapacityId"/>,
/// which describes the capacity of a sellable storage medium (SD card, USB drive). Both reference
/// the same <see cref="StorageCapacity"/> lookup table but mean different things - do not confuse them.
/// </remarks>
[Table("DeviceSpecs")]
public class DeviceSpec : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    /// <summary>Installed RAM, in gigabytes (GB).</summary>
    [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public int Ram { get; set; }

    /// <summary>Built-in storage capacity (see the type-level remarks).</summary>
    public int StorageCapacityId { get; set; }
    public virtual StorageCapacity StorageCapacityNavigation { get; set; } = null!;

    /// <summary>Charging speed, in watts (W).</summary>
    [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public int ChargeSpeed { get; set; }
}