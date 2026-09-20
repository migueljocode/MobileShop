namespace MobileShop.Models.Entities.Configuration;

public class SmartWatchConfiguration : IEntityTypeConfiguration<SmartWatch>
{
    public void Configure(EntityTypeBuilder<SmartWatch> builder)
    {
        builder.HasOne(w => w.ProductNavigation)
            .WithOne(product => product.SmartWatchProfile)
            .HasForeignKey<SmartWatch>(w => w.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}