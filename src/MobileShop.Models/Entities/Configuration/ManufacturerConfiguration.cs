namespace MobileShop.Models.Entities.Configuration;

public class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        // unique, but only among live records - otherwise a soft-deleted manufacturer would lock its name forever
        builder.HasIndex(m => m.Name)
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */

        builder.HasMany(m => m.Models)
            .WithOne(model => model.ManufacturerNavigation)
            .HasForeignKey(model => model.ManufacturerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}