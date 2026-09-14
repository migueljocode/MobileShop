namespace MobileShop.Web.Pages;

public class IndexModel(
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService,
    ITransactionDataService transactionDataService) : PageModel
{
    public DashboardStockSummary Stock { get; private set; } = new(0, 0, 0, 0);
    public IReadOnlyList<TransactionCardViewModel> RecentTransactions { get; private set; } = [];

    public void OnGet()
    {
        Stock = new DashboardStockSummary(
            phoneDataService.Quantity(),
            phoneDataService.SecondHandQuantity(),
            phoneDataService.AvailableSecondHandQuantity(),
            appleIdDataService.Quantity());
        RecentTransactions = transactionDataService.GetRecentCards();
    }
}
