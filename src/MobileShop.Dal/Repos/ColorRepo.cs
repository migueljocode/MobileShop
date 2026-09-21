namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IColorRepo" />
public class ColorRepo(AppDbContext context) : BaseRepo<Color>(context), IColorRepo { }