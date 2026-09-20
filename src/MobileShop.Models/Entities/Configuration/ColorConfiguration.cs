namespace MobileShop.Models.Entities.Configuration;

public class ColorConfiguration : IEntityTypeConfiguration<Color>
{
    public void Configure(EntityTypeBuilder<Color> builder)
    {
        builder.HasMany(c => c.Products)
            .WithOne(product => product.ColorNavigation)
            .HasForeignKey(product => product.ColorId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}