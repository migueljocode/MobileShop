namespace MobileShop.Models.Entities.Configuration;

public class ShopProfileConfiguration : IEntityTypeConfiguration<ShopProfile>
{
    public void Configure(EntityTypeBuilder<ShopProfile> builder)
    {
        builder.ToTable("ShopProfiles");
    }
}
