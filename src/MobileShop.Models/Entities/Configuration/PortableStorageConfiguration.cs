namespace MobileShop.Models.Entities.Configuration;

public class PortableStorageConfiguration : IEntityTypeConfiguration<PortableStorage>
{
    public void Configure(EntityTypeBuilder<PortableStorage> builder)
    {
        builder.HasOne(s => s.ProductNavigation)
            .WithOne(product => product.PortableStorageProfile)
            .HasForeignKey<PortableStorage>(s => s.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        // capacity of the medium itself - see the remarks on PortableStorage
        builder.HasOne(s => s.StorageCapacityNavigation)
            .WithMany(capacity => capacity.PortableStorages)
            .HasForeignKey(s => s.StorageCapacityId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}