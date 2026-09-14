namespace MobileShop.Models.Entities.Configuration;

public class PhoneConfiguration : IEntityTypeConfiguration<Phone>
{
    public void Configure(EntityTypeBuilder<Phone> builder)
    {
        // unique, but only among live records - otherwise a soft-deleted phone would lock its IMEI forever
        builder.HasIndex(p => p.IMEI1)
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */

        builder.Property(p => p.Color)
            .HasMaxLength(50);

        builder.HasOne(p => p.ProductNavigation)
            .WithOne(product => product.PhoneProfile)
            .HasForeignKey<Phone>(p => p.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
