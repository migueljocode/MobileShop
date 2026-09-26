namespace MobileShop.Web.Pages;

    public class IndexModel(
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService,
    ITransactionDataService transactionDataService) : PageModel
{
    public DashboardStockSummary Stock { get; private set; } = new(0, 0, 0, 0);
    public IReadOnlyList<TransactionCardViewModel> RecentTransactions { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Stock = new DashboardStockSummary(
            await phoneDataService.QuantityAsync(),
            await phoneDataService.SecondHandQuantityAsync(),
            await phoneDataService.AvailableSecondHandQuantityAsync(),
            await appleIdDataService.QuantityAsync());
        RecentTransactions = await transactionDataService.GetRecentCardsAsync();
    }
}
