namespace MobileShop.Models.Entities.Configuration;

public class ChargerConfiguration : IEntityTypeConfiguration<Charger>
{
    public void Configure(EntityTypeBuilder<Charger> builder)
    {
        builder.HasOne(c => c.ProductNavigation)
            .WithOne(product => product.ChargerProfile)
            .HasForeignKey<Charger>(c => c.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}