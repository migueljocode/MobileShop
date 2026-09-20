namespace MobileShop.Models.Entities.Configuration;

public class CaseModelFitConfiguration : IEntityTypeConfiguration<CaseModelFit>
{
    public void Configure(EntityTypeBuilder<CaseModelFit> builder)
    {
        // unique, but only among live records - otherwise a soft-deleted link would block re-linking
        builder.HasIndex(f => new { f.CaseId, f.ModelId })
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */

        builder.HasOne(f => f.CaseNavigation)
            .WithMany(c => c.ModelFits)
            .HasForeignKey(f => f.CaseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(f => f.ModelNavigation)
            .WithMany(model => model.CaseFits)
            .HasForeignKey(f => f.ModelId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}