namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides dashboard stock counts and recent transaction cards.</summary>
public class HomeDataService(
    IBaseRepo<Phone> phones,
    IBaseRepo<AppleId> appleIds,
    IBaseRepo<Transaction> transactions,
    ILogger<HomeDataService> logger)
    : IHomeDataService
{
    /// <summary>Gets the structured logger for this dashboard service.</summary>
    protected ILogger<HomeDataService> Logger { get; } = logger;

    /// <inheritdoc />
    public async Task<DashboardStockSummary> GetStockAsync()
        => new(
            await phones.CountAsync(),
            await phones.CountAsync(phone => phone.ProductNavigation.SecondHandProfile != null),
            await phones.CountAsync(phone =>
                phone.ProductNavigation.SecondHandProfile != null &&
                !phone.ProductNavigation.Transactions.Any(transaction => transaction.Direction == TransactionDirection.Sell)),
            await appleIds.CountAsync());

    /// <inheritdoc />
    public async Task<IReadOnlyList<TransactionCardViewModel>> GetRecentTransactionsAsync(int count = 20)
    {
        var cards = await transactions.SelectAllAsync(transaction => new TransactionCardViewModel(
            transaction.Date,
            transaction.Direction,
            transaction.FinishedPrice,
            transaction.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + transaction.ProductNavigation.ModelNavigation.Name,
            transaction.Direction == TransactionDirection.Buy
                ? "From: " + (transaction.SellerNavigation.PersonNavigation == null
                    ? "Shop"
                    : transaction.SellerNavigation.PersonNavigation.FirstName + " " + transaction.SellerNavigation.PersonNavigation.LastName)
                : "To: " + (transaction.CustomerNavigation.PersonNavigation == null
                    ? "Shop"
                    : transaction.CustomerNavigation.PersonNavigation.FirstName + " " + transaction.CustomerNavigation.PersonNavigation.LastName)));

        return cards
            .OrderByDescending(card => card.Date)
            .Take(count)
            .ToList();
    }
}
