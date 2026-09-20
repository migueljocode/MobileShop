namespace MobileShop.Models.Entities.Configuration;

public class TabletConfiguration : IEntityTypeConfiguration<Tablet>
{
    public void Configure(EntityTypeBuilder<Tablet> builder)
    {
        builder.HasOne(t => t.ProductNavigation)
            .WithOne(product => product.TabletProfile)
            .HasForeignKey<Tablet>(t => t.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}