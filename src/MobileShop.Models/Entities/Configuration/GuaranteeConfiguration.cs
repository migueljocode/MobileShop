namespace MobileShop.Models.Entities.Configuration;

public class GuaranteeConfiguration : IEntityTypeConfiguration<Guarantee>
{
    public void Configure(EntityTypeBuilder<Guarantee> builder)
    {
        builder.HasOne(g => g.ProductNavigation)
            .WithOne(product => product.GuaranteeProfile)
            .HasForeignKey<Guarantee>(g => g.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
