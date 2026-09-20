namespace MobileShop.Models.Entities.Configuration;

public class GlassConfiguration : IEntityTypeConfiguration<Glass>
{
    public void Configure(EntityTypeBuilder<Glass> builder)
    {
        builder.HasOne(g => g.ProductNavigation)
            .WithOne(product => product.GlassProfile)
            .HasForeignKey<Glass>(g => g.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}