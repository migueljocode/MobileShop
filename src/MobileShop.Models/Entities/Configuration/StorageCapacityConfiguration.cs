namespace MobileShop.Models.Entities.Configuration;

public class StorageCapacityConfiguration : IEntityTypeConfiguration<StorageCapacity>
{
    public void Configure(EntityTypeBuilder<StorageCapacity> builder)
    {
        // unique, but only among live records - otherwise a soft-deleted capacity would lock its size forever
        builder.HasIndex(s => s.Gb)
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */

        // this single lookup is referenced from two different places with two different meanings:
        //   DeviceSpec.StorageCapacityId      - the built-in capacity of a device
        //   PortableStorage.StorageCapacityId - the capacity of a sellable storage medium
        // both relationships are configured on those (dependent) entities
    }
}