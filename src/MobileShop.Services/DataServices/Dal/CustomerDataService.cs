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

    /// <inheritdoc />
    public async Task<IReadOnlyList<PartyOptionViewModel>> GetPartyOptionsAsync()
        => (await _customerRepo
            .SelectAllAsync(customer => new PartyOptionViewModel(
                customer.Id,
                customer.PersonNavigation.FirstName + " " + customer.PersonNavigation.LastName,
                customer.PersonNavigation.PhoneNumber)))
            .OrderBy(row => row.Label)
            .ToList();

        /// <inheritdoc />
    public IReadOnlyList<CustomerListItemViewModel> GetListRows()
        => _customerRepo
            .SelectAll(customer => new CustomerListItemViewModel(
                customer.Id,
                customer.PersonNavigation.FirstName + " " + customer.PersonNavigation.LastName,
                customer.PersonNavigation.PhoneNumber,
                customer.NationalId))
            .OrderBy(row => row.Name)
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<CustomerListItemViewModel>> GetListRowsAsync()
        => (await _customerRepo
            .SelectAllAsync(customer => new CustomerListItemViewModel(
                customer.Id,
                customer.PersonNavigation.FirstName + " " + customer.PersonNavigation.LastName,
                customer.PersonNavigation.PhoneNumber,
                customer.NationalId)))
            .OrderBy(row => row.Name)
            .ToList();

    /// <inheritdoc />
    public CustomerDetailsViewModel? GetDetails(int id)
        => _customerRepo.Select(
            id,
            customer => new CustomerDetailsViewModel(
                customer.PersonNavigation.FirstName + " " + customer.PersonNavigation.LastName,
                customer.PersonNavigation.PhoneNumber,
                customer.NationalId));

    /// <inheritdoc />
    public Task<CustomerDetailsViewModel?> GetDetailsAsync(int id)
        => _customerRepo.SelectAsync(
            id,
            customer => new CustomerDetailsViewModel(
                customer.PersonNavigation.FirstName + " " + customer.PersonNavigation.LastName,
                customer.PersonNavigation.PhoneNumber,
                customer.NationalId));

    // ── Purchased products ────────────────────────────────

    /// <inheritdoc />
    public IEnumerable<Product> PurchasedProducts(int customerId)
        => _customerRepo
            .SelectAll(
                customer => customer.Id == customerId,
                customer => customer.Transactions
                    .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                    .Select(transaction => transaction.ProductNavigation)
                    .ToList())
            .SelectMany(products => products)
            .Distinct()
            .ToList();

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> PurchasedProductsAsync(int customerId)
        => (await _customerRepo.SelectAllAsync(
                customer => customer.Id == customerId,
                customer => customer.Transactions
                    .Where(transaction => transaction.Direction == TransactionDirection.Sell)
                    .Select(transaction => transaction.ProductNavigation)
                    .ToList()))
            .SelectMany(products => products)
            .Distinct()
            .ToList();

}