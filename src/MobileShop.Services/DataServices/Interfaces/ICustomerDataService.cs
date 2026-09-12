namespace MobileShop.Services.DataServices.Interfaces;

public interface ICustomerDataService : IDataService<Customer>
{
    // TODO: eager-load Product details when history page needs them
    IEnumerable<Product> PurchasedProducts(int customerId);
    IEnumerable<Product> PurchasedProducts(int customerId, Expression<Func<Product, bool>> predicate);
    Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId);
    Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId, Expression<Func<Product, bool>> predicate);
}