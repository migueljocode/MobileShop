namespace MobileShop.Services.DataServices.Dal;

public class CustomerDataService(
    ICustomerRepo customerRepo,
    ILogger<CustomerDataService> logger)
    : DataServiceBase<CustomerDataService, Customer>(customerRepo, logger), ICustomerDataService
{
    private readonly ICustomerRepo _customerRepo = customerRepo;

    // ── Purchased products ────────────────────────────────

    public IEnumerable<Product> PurchasedProducts(int customerId)
        => _customerRepo.PurchasedProducts(customerId) ?? [];

    public IEnumerable<Product> PurchasedProducts(
        int customerId,
        Expression<Func<Product, bool>> predicate)
    {
        var products = PurchasedProducts(customerId);
        return products.AsQueryable().Where(predicate).ToList();
    }

    public async Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId)
        => await _customerRepo.PurchasedProductsAsync(customerId) ?? [];

    public async Task<IEnumerable<Product>> PurchasedProductsAsync(
        int customerId,
        Expression<Func<Product, bool>> predicate)
    {
        var products = await PurchasedProductsAsync(customerId);
        return products.AsQueryable().Where(predicate).ToList();
    }
}