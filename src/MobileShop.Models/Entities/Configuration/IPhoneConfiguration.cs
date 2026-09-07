namespace MobileShop.Models.Entities.Configuration;

public class IPhoneConfiguration : IEntityTypeConfiguration<IPhone>
{
    public void Configure(EntityTypeBuilder<IPhone> builder)
    {
        builder.HasOne(i => i.PhoneNavigation)
            .WithOne(p => p.IPhoneProfile)
            .HasForeignKey<IPhone>(i => i.PhoneId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
