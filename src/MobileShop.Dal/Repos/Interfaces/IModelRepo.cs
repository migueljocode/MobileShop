namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="Model"/> entities.</summary>
public interface IModelRepo : IBaseRepo<Model>
{
    /// <summary>Gets all models for a manufacturer.</summary>
    /// <param name="manufacturerId">The manufacturer identifier.</param>
    Task<IEnumerable<Model>> GetByManufacturerAsync(int manufacturerId);
}