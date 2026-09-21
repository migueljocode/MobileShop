namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IModelRepo" />
public class ModelRepo(AppDbContext context) : BaseRepo<Model>(context), IModelRepo { }