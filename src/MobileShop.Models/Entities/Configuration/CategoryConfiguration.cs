namespace MobileShop.Models.Entities.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasMany(c => c.Models)
            .WithOne(model => model.CategoryNavigation)
            .HasForeignKey(model => model.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}