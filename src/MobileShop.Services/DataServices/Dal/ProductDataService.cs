namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides catalog-wide product inventory projections.</summary>
public sealed class ProductDataService(
    IProductRepo productRepo,
    ILogger<ProductDataService> logger)
    : DataServiceBase<ProductDataService, Product>(productRepo, logger), IProductDataService
{
    private readonly IProductRepo _productRepo = productRepo;

    /// <inheritdoc />
    public IReadOnlyList<ProductListItemViewModel> GetInventoryRows()
        => _productRepo.SelectAll(product => new ProductListItemViewModel(
                product.Id,
                product.Id,
                product.ModelNavigation.CategoryNavigation.Name,
                product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name,
                product.Barcode,
                product.ColorNavigation == null ? null : product.ColorNavigation.Name,
                product.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell),
                product.SecondHandProfile != null))
            .OrderBy(row => row.ProductId)
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync()
        => (await _productRepo.SelectAllAsync(product => new ProductListItemViewModel(
                product.Id,
                product.Id,
                product.ModelNavigation.CategoryNavigation.Name,
                product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name,
                product.Barcode,
                product.ColorNavigation == null ? null : product.ColorNavigation.Name,
                product.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell),
                product.SecondHandProfile != null)))
            .OrderBy(row => row.ProductId)
            .ToList();
}
