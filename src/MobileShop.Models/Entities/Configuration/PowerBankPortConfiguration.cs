namespace MobileShop.Models.Entities.Configuration;

public class PowerBankPortConfiguration : IEntityTypeConfiguration<PowerBankPort>
{
    public void Configure(EntityTypeBuilder<PowerBankPort> builder)
    {
        builder.HasOne(port => port.PowerBankNavigation)
            .WithMany(powerBank => powerBank.Ports)
            .HasForeignKey(port => port.PowerBankId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(port => new { port.PowerBankId, port.PortNumber })
            .IsUnique();
    }
}
