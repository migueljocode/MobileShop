namespace MobileShop.Dal.Repos.Interfaces;

/// <summary>Repository for <see cref="Customer"/> entities.</summary>
public interface ICustomerRepo : IBaseRepo<Customer>
{
    IEnumerable<Product>? PurchasedProducts(int customerId);
    Task<IEnumerable<Product>?> PurchasedProductsAsync(int customerId);
}