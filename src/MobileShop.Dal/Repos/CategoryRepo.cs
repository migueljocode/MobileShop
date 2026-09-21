namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ICategoryRepo" />
public class CategoryRepo(AppDbContext context) : BaseRepo<Category>(context), ICategoryRepo { }