namespace MobileShop.Models.Entities.Configuration;

public class SellerConfiguration : IEntityTypeConfiguration<Seller>
{
    public void Configure(EntityTypeBuilder<Seller> builder)
    {
        builder.HasOne(s => s.PersonNavigation)
            .WithOne(p => p.SellerProfile)
            .HasForeignKey<Seller>(s => s.PersonId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
