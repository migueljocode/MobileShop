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

    public void OnGet()
    {
        Rows = transactionDataService.GetProfitLossRows(From, To);
        TotalProfit = transactionDataService.GetProfitLossTotal(From, To);

        // Calculate distribution
        var employees = employeeDataService.FindAll().Where(e => e.IsActive).ToList();
        DistributionRows = DistributionCalculator.Calculate(TotalProfit, employees, distributionSettings.Value);
    }
}
