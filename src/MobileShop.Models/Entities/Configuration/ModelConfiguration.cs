namespace MobileShop.Models.Entities.Configuration;

public class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        // unique, but only among live records - otherwise a soft-deleted model would lock its name forever
        builder.HasIndex(m => new { m.ManufacturerId, m.Name })
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */

        builder.HasOne(m => m.ManufacturerNavigation)
            .WithMany(manufacturer => manufacturer.Models)
            .HasForeignKey(m => m.ManufacturerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(m => m.CategoryNavigation)
            .WithMany(category => category.Models)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        // the case/glass "fits" relationships are configured on their own join entities
        builder.HasMany(m => m.Products)
            .WithOne(product => product.ModelNavigation)
            .HasForeignKey(product => product.ModelId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}