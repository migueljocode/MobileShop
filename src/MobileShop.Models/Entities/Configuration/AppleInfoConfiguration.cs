namespace MobileShop.Models.Entities.Configuration;

public class AppleInfoConfiguration : IEntityTypeConfiguration<AppleInfo>
{
    public void Configure(EntityTypeBuilder<AppleInfo> builder)
    {
        builder.HasOne(a => a.ProductNavigation)
            .WithOne(product => product.AppleInfoProfile)
            .HasForeignKey<AppleInfo>(a => a.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}