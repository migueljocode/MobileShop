namespace MobileShop.Web.Pages;

public class IndexModel(IHomeDataService dataService) : PageModel
{
    public DashboardStockSummary Stock { get; private set; } = new(0, 0, 0, 0);
    public IReadOnlyList<TransactionCardViewModel> RecentTransactions { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Stock = await dataService.GetStockAsync();
        RecentTransactions = await dataService.GetRecentTransactionsAsync();
    }
}
