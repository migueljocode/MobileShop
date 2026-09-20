namespace MobileShop.Models.Entities.Configuration;

public class GlassModelFitConfiguration : IEntityTypeConfiguration<GlassModelFit>
{
    public void Configure(EntityTypeBuilder<GlassModelFit> builder)
    {
        // unique, but only among live records - otherwise a soft-deleted link would block re-linking
        builder.HasIndex(f => new { f.GlassId, f.ModelId })
            .IsUnique()
            .HasFilter("IsDeleted = 0"); /* SQLite */

        builder.HasOne(f => f.GlassNavigation)
            .WithMany(g => g.ModelFits)
            .HasForeignKey(f => f.GlassId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(f => f.ModelNavigation)
            .WithMany(model => model.GlassFits)
            .HasForeignKey(f => f.ModelId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}