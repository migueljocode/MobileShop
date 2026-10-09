using System.Text.RegularExpressions;
using MobileShop.Services.DataServices.Shared;

namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the catalog and creation operations for the Products area.</summary>
/// <remarks>
/// The repositories cover product types, shared catalog entities (including CPU/GPU lookups), and transactions. There is no
/// <see cref="Product"/> repo because each create path builds the product row inline and it is persisted by
/// cascade through the phone or Apple-ID insert, and no <see cref="SecondHand"/> repo (the second-hand profile is
/// a <see cref="Product"/> navigation, not a separate write).
/// </remarks>
public class ProductsDataService(
    IBaseRepo<Phone> phones,
    IBaseRepo<AppleId> appleIds,
    IBaseRepo<Seller> sellers,
    IBaseRepo<Customer> customers,
    IBaseRepo<Person> people,
    IBaseRepo<Manufacturer> manufacturers,
    IBaseRepo<Model> models,
    IBaseRepo<Category> categories,
    IBaseRepo<MobileShop.Models.Entities.Color> colors,
    IBaseRepo<Guarantee> guarantees,
    IBaseRepo<Transaction> transactions,
    IBaseRepo<PartNumber> partNumbers,
    IBaseRepo<Product> products,
    IBaseRepo<StorageCapacity> storageCapacities,
    IBaseRepo<Cpu> cpus,
    IBaseRepo<Gpu> gpus,
    ILogger<ProductsDataService> logger)
    : IProductsDataService
{
    // Product creation represents stock intake, so its paid cost is recorded as a Buy leg.
    private const string PhoneCategoryName = "Phone";
        /// <summary>Gets the structured logger for this products service.</summary>
    protected ILogger<ProductsDataService> Logger { get; } = logger;

    // TEMP STUB - will be replaced with full content in next push if this works
    public Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync(string? type = null, int? partNumberId = null, string? availability = null)
        => throw new NotImplementedException("RESTORE IN PROGRESS");

    public Task<IReadOnlyList<ProductListItemViewModel>> GetSecondHandRowsAsync()
        => throw new NotImplementedException("RESTORE IN PROGRESS");

    public Task<ProductDetailsViewModel?> GetDetailsAsync(int id, string type)
        => throw new NotImplementedException("RESTORE IN PROGRESS");

    public Task<ServiceResult> UpdateProductAsync(EditProductInputModel input)
        => throw new NotImplementedException("RESTORE IN PROGRESS");

    public Task<Product?> GetProductForEditAsync(int id)
        => throw new NotImplementedException("RESTORE IN PROGRESS");
}
