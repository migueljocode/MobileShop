namespace MobileShop.Models.Entities.Configuration;

public class CableConfiguration : IEntityTypeConfiguration<Cable>
{
    public void Configure(EntityTypeBuilder<Cable> builder)
    {
        builder.HasOne(c => c.ProductNavigation)
            .WithOne(product => product.CableProfile)
            .HasForeignKey<Cable>(c => c.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}