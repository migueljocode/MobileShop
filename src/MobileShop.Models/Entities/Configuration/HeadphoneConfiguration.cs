namespace MobileShop.Models.Entities.Configuration;

public class HeadphoneConfiguration : IEntityTypeConfiguration<Headphone>
{
    public void Configure(EntityTypeBuilder<Headphone> builder)
    {
        builder.HasOne(h => h.ProductNavigation)
            .WithOne(product => product.HeadphoneProfile)
            .HasForeignKey<Headphone>(h => h.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
