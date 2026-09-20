namespace MobileShop.Models.Entities.Configuration;

public class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.HasOne(c => c.ProductNavigation)
            .WithOne(product => product.CaseProfile)
            .HasForeignKey<Case>(c => c.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}