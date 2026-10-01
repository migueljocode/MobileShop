namespace MobileShop.Models.Entities.Configuration;

public class PartNumberConfiguration : IEntityTypeConfiguration<PartNumber>
{
    public void Configure(EntityTypeBuilder<PartNumber> builder)
    {
        // unique per Model + Code among live records (soft-deleted entries do not block reuse)
        builder.HasIndex(p => new { p.ModelId, p.Code })
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */

        builder.HasOne(p => p.ModelNavigation)
            .WithMany(m => m.PartNumbers)
            .HasForeignKey(p => p.ModelId)
            .OnDelete(DeleteBehavior.NoAction);

        // Phone -> PartNumber is optional; no cascade delete so existing phones are unaffected
        builder.HasMany(p => p.Phones)
            .WithOne(phone => phone.PartNumberNavigation)
            .HasForeignKey(phone => phone.PartNumberId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
