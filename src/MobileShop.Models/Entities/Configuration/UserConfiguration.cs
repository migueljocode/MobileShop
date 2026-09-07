namespace MobileShop.Models.Entities.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */

        builder.HasOne(u => u.PersonNavigation)
            .WithOne(p => p.UserProfile)
            .HasForeignKey<User>(u => u.PersonId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
