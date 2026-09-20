namespace MobileShop.Models.Entities.Configuration;

public class LaptopConfiguration : IEntityTypeConfiguration<Laptop>
{
    public void Configure(EntityTypeBuilder<Laptop> builder)
    {
        builder.HasOne(l => l.ProductNavigation)
            .WithOne(product => product.LaptopProfile)
            .HasForeignKey<Laptop>(l => l.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}