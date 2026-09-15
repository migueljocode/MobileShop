namespace MobileShop.Services.DataServices.Dal;

public class CustomerDataService(
    ICustomerRepo customerRepo,
    ILogger<CustomerDataService> logger)
    : DataServiceBase<CustomerDataService, Customer>(customerRepo, logger), ICustomerDataService
{
    private readonly ICustomerRepo _customerRepo = customerRepo;

    /// <inheritdoc />
    public IReadOnlyList<PartyOptionViewModel> GetPartyOptions()
        => _customerRepo
            .SelectAll(customer => new PartyOptionViewModel(
                customer.Id,
                customer.PersonNavigation.FirstName + " " + customer.PersonNavigation.LastName,
                customer.PersonNavigation.PhoneNumber))
            .OrderBy(row => row.Label)
            .ToList();

    // ── Purchased products ────────────────────────────────

    /// <inheritdoc />
    public IEnumerable<Product> PurchasedProducts(int customerId)
        => _customerRepo
            .FindAll(customer => customer.Id == customerId)
            .SelectMany(customer => customer.Transactions)
            .Where(transaction => transaction.Direction == TransactionDirection.Sell)
            .Select(transaction => transaction.ProductNavigation)
            .Distinct()
            .ToList();

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId)
        => (await _customerRepo.FindAllAsync(customer => customer.Id == customerId))
            .SelectMany(customer => customer.Transactions)
            .Where(transaction => transaction.Direction == TransactionDirection.Sell)
            .Select(transaction => transaction.ProductNavigation)
            .Distinct()
            .ToList();

}