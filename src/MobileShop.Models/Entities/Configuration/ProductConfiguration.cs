namespace MobileShop.Models.Entities.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // defense in depth - Range on the property is only enforced by app-level validation (e.g. ModelState);
        // this enforces the same rule at the database level regardless of how a row gets written
        builder.ToTable(t => t.HasCheckConstraint("CK_Products_Price_NonNegative", "Price >= 0"));

        // unique, but only among live records - otherwise a soft-deleted product would lock its barcode forever
        builder.HasIndex(p => p.Barcode)
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */

        builder.HasOne(p => p.ModelNavigation)
            .WithMany(model => model.Products)
            .HasForeignKey(p => p.ModelId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.ColorNavigation)
            .WithMany(color => color.Products)
            .HasForeignKey(p => p.ColorId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
