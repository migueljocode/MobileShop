namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IModelRepo" />
public class ModelRepo(AppDbContext context) : BaseRepo<Model>(context), IModelRepo
{
    public async Task<IEnumerable<Model>> GetByManufacturerAsync(int manufacturerId)
        => await FindAllAsync(m => m.ManufacturerId == manufacturerId);
}