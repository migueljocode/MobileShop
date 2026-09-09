namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ICustomerRepo" />
public class CustomerRepo(AppDbContext context) : BaseRepo<Customer>(context), ICustomerRepo
{
    public override Customer? Find(int id)
        =>  Table.Include(x => x.PersonNavigation)
                .FirstOrDefault(x => x.Id == id);
    public override async Task<Customer?> FindAsync(int id)
        => await Table.Include(x => x.PersonNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);

    public IEnumerable<Product>? PurchasedProducts(int customerId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Product>?> PurchasedProductsAsync(int customerId)
    {
        throw new NotImplementedException();
    }
}