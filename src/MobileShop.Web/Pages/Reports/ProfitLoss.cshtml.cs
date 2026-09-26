namespace MobileShop.Web.Pages.Reports;

public class ProfitLossModel(
    ITransactionDataService transactionDataService,
    IEmployeeDataService employeeDataService,
    IOptions<DistributionSettings> distributionSettings) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }
    public IReadOnlyList<ProfitLossRowViewModel> Rows { get; private set; } = [];
    public decimal TotalProfit { get; private set; }

    // Distribution
    public IReadOnlyList<DistributionRow> DistributionRows { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Rows = await transactionDataService.GetProfitLossRowsAsync(From, To);
        TotalProfit = await transactionDataService.GetProfitLossTotalAsync(From, To);

        // Calculate distribution
        var employees = await employeeDataService.GetActiveEmployeesAsync();
        DistributionRows = DistributionCalculator.Calculate(TotalProfit, employees, distributionSettings.Value);
    }
}
