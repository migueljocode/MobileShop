namespace MobileShop.Models.Entities.Configuration;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.HasOne(p => p.SellerNavigation)
            .WithMany(s => s.Purchases)
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
