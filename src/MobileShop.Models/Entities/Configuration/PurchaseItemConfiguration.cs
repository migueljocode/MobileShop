namespace MobileShop.Models.Entities.Configuration;

public class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<PurchaseItem> builder)
    {
        builder.HasOne(pi => pi.PurchaseNavigation)
            .WithMany(p => p.Items)
            .HasForeignKey(pi => pi.PurchaseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(pi => pi.StockGoodNavigation)
            .WithMany()
            .HasForeignKey(pi => pi.StockGoodId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
