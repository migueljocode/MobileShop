namespace MobileShop.Models.Entities.Configuration;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasOne(c => c.PersonNavigation)
            .WithOne(p => p.CustomerProfile)
            .HasForeignKey<Customer>(c => c.PersonId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
