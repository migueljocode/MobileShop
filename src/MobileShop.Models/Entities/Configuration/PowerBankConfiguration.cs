namespace MobileShop.Models.Entities.Configuration;

public class PowerBankConfiguration : IEntityTypeConfiguration<PowerBank>
{
    public void Configure(EntityTypeBuilder<PowerBank> builder)
    {
        builder.HasOne(p => p.ProductNavigation)
            .WithOne(product => product.PowerBankProfile)
            .HasForeignKey<PowerBank>(p => p.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}