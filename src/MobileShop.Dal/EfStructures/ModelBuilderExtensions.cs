namespace MobileShop.Dal.EfStructures;

/// <summary>
/// Provides common EF Core model configuration for MobileShop entities.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies a global query filter that hides soft-deleted entities.
    /// </summary>
    public static ModelBuilder ApplySoftDeleteForEntities(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var isDeleted = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(isDeleted), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        return modelBuilder;
    }
}
