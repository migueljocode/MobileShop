namespace MobileShop.Models.Entities.Configuration;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.HasOne(si => si.SaleNavigation)
            .WithMany(s => s.Items)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(si => si.StockGoodNavigation)
            .WithMany()
            .HasForeignKey(si => si.StockGoodId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
