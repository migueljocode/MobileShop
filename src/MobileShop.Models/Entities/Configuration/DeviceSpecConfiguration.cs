namespace MobileShop.Models.Entities.Configuration;

public class DeviceSpecConfiguration : IEntityTypeConfiguration<DeviceSpec>
{
    public void Configure(EntityTypeBuilder<DeviceSpec> builder)
    {
        builder.HasOne(d => d.ProductNavigation)
            .WithOne(product => product.DeviceSpecProfile)
            .HasForeignKey<DeviceSpec>(d => d.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        // built-in capacity of the device - see the remarks on DeviceSpec
        builder.HasOne(d => d.StorageCapacityNavigation)
            .WithMany(capacity => capacity.DeviceSpecs)
            .HasForeignKey(d => d.StorageCapacityId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}