namespace MobileShop.Models.Entities;

/// <summary>
/// A sellable portable storage device (SD card, USB flash drive, external SSD/HDD). One-to-one
/// with <see cref="Product"/>.
/// </summary>
/// <remarks>
/// <see cref="StorageCapacityId"/> is the capacity <b>of the storage medium itself</b> - the thing
/// the customer is buying. This is deliberately a different relationship from
/// <see cref="DeviceSpec.StorageCapacityId"/>, which describes a device's <b>built-in</b> storage
/// (e.g. a phone's 128 GB of internal memory). Both point at the same
/// <see cref="StorageCapacity"/> lookup table, but they mean different things - do not confuse them.
/// </remarks>
[Table("PortableStorages")]
public class PortableStorage : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    public StorageKind Kind { get; set; }

    /// <summary>Capacity of the medium itself (see the type-level remarks).</summary>
    public int StorageCapacityId { get; set; }
    public virtual StorageCapacity StorageCapacityNavigation { get; set; } = null!;

    /// <summary>Read/write speed in megabytes per second (MB/s).</summary>
    [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public int? Speed { get; set; }

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }
}