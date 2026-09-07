namespace MobileShop.Models.Entities.Configuration;

public class AppleIdConfiguration : IEntityTypeConfiguration<AppleId>
{
    public void Configure(EntityTypeBuilder<AppleId> builder)
    {
        builder.HasOne(a => a.ProductNavigation)
            .WithOne(product => product.AppleIdProfile)
            .HasForeignKey<AppleId>(a => a.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
