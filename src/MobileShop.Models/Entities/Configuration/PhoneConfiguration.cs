namespace MobileShop.Models.Entities.Configuration;

public class PhoneConfiguration : IEntityTypeConfiguration<Phone>
{
    public void Configure(EntityTypeBuilder<Phone> builder)
    {
        // یونیک، ولی فقط رو رکوردهای زنده - وگرنه یه گوشی soft-deleted شده IMEI رو برای همیشه قفل می‌کنه
        builder.HasIndex(p => p.IMEI1)
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */
            // .HasFilter("[IsDeleted] = 0"); /* SqlServer */
            // .HasFilter("\"IsDeleted\" = false"); /* PostgreSQL */


        builder.HasOne(p => p.PurchaseNavigation)
            .WithMany(pu => pu.Phones)
            .HasForeignKey(p => p.PurchaseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.SaleNavigation)
            .WithMany(s => s.Phones)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.CustomerNavigation)
            .WithMany(c => c.PurchasedPhones)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(p => p.AppleIdNavigation)
            .WithMany(a => a.Phones)
            .HasForeignKey(p => p.AppleIdId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
