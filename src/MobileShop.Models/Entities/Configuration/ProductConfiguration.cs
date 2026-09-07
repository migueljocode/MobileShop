namespace MobileShop.Models.Entities.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // defense in depth - Range on the property is only enforced by app-level validation (e.g. ModelState);
        // this enforces the same rule at the database level regardless of how a row gets written
        builder.ToTable(t => t.HasCheckConstraint("CK_Products_Price_NonNegative", "Price >= 0"));
    }
}
