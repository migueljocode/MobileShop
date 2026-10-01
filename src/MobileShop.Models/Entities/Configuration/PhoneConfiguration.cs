namespace MobileShop.Models.Entities.Configuration;

public class PhoneConfiguration : IEntityTypeConfiguration<Phone>
{
    public void Configure(EntityTypeBuilder<Phone> builder)
    {
        // unique, but only among live records - otherwise a soft-deleted phone would lock its IMEI forever
        builder.HasIndex(p => p.IMEI1)
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */

        builder.HasOne(p => p.ProductNavigation)
            .WithOne(product => product.PhoneProfile)
            .HasForeignKey<Phone>(p => p.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        // Phone -> PartNumber is optional; no cascade delete
        builder.HasOne(p => p.PartNumberNavigation)
            .WithMany(pn => pn.Phones)
            .HasForeignKey(p => p.PartNumberId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
