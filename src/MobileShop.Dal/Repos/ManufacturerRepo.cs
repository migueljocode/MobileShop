namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IManufacturerRepo" />
public class ManufacturerRepo(AppDbContext context) : BaseRepo<Manufacturer>(context), IManufacturerRepo { }
