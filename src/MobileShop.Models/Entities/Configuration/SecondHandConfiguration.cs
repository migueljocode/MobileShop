namespace MobileShop.Models.Entities.Configuration;

public class SecondHandConfiguration : IEntityTypeConfiguration<SecondHand>
{
    public void Configure(EntityTypeBuilder<SecondHand> builder)
    {
        builder.HasOne(sh => sh.ProductNavigation)
            .WithOne(product => product.SecondHandProfile)
            .HasForeignKey<SecondHand>(sh => sh.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
