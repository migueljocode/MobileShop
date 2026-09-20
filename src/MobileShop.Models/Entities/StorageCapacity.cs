namespace MobileShop.Models.Entities;

/// <summary>
/// Shared capacity lookup, referenced by both <see cref="DeviceSpec"/> (a device's built-in
/// storage) and <see cref="PortableStorage"/> (a sellable storage device's own capacity).
/// The gigabyte figure lives here only - it is never duplicated on the referencing entities.
/// </summary>
[Table("StorageCapacities")]
public class StorageCapacity : BaseEntity
{
    /// <summary>Capacity in gigabytes (GB).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "{0} must be greater than zero.")]
    public int Gb { get; set; }

    public virtual ICollection<DeviceSpec> DeviceSpecs { get; set; } = [];
    public virtual ICollection<PortableStorage> PortableStorages { get; set; } = [];
}