namespace MobileShop.Web.Pages.Reports;

public enum DateRangeMode { Automatic, Manual }
public enum AutomaticPreset { Today, Week, Month, Year }

public class ProfitLossModel(
    ITransactionDataService transactionDataService,
    IEmployeeDataService employeeDataService,
    IOptions<DistributionSettings> distributionSettings) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateRangeMode Mode { get; set; } = DateRangeMode.Automatic;
    [BindProperty(SupportsGet = true)] public AutomaticPreset Preset { get; set; } = AutomaticPreset.Month;

    [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }

    public DateTime EffectiveFrom { get; private set; }
    public DateTime EffectiveTo { get; private set; }
    public string? EmptyDatabaseNote { get; private set; }

    public IReadOnlyList<ProfitLossRowViewModel> Rows { get; private set; } = [];
    public decimal TotalProfit { get; private set; }

    // Distribution
    public IReadOnlyList<DistributionRow> DistributionRows { get; private set; } = [];

    private async Task ResolveBoundsAsync()
    {
        var today = DateTime.Today;

        if (Mode == DateRangeMode.Automatic)
        {
            var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
            (EffectiveFrom, EffectiveTo) = Preset switch
            {
                AutomaticPreset.Today => (today, today),
                AutomaticPreset.Week => (today.AddDays(-daysSinceMonday), today),
                AutomaticPreset.Month => (new DateTime(today.Year, today.Month, 1), today),
                AutomaticPreset.Year => (new DateTime(today.Year, 1, 1), today),
                _ => (new DateTime(today.Year, today.Month, 1), today)
            };
        }
        else
        {
            var earliest = await transactionDataService.GetEarliestTransactionDateAsync();
            EffectiveFrom = From ?? (earliest ?? today);
            EffectiveTo = To ?? today;

            // Write defaults back so date pickers show the same values used for filtering
            From = EffectiveFrom;
            To = EffectiveTo;

            if (earliest == null)
                EmptyDatabaseNote = "No transactions recorded yet. Date range defaults to today.";
        }
    }

    public async Task OnGetAsync()
    {
        await ResolveBoundsAsync();

        Rows = await transactionDataService.GetProfitLossRowsAsync(EffectiveFrom, EffectiveTo);
        TotalProfit = await transactionDataService.GetProfitLossTotalAsync(EffectiveFrom, EffectiveTo);

        // Calculate distribution
        var employees = await employeeDataService.GetActiveEmployeesAsync();
        DistributionRows = DistributionCalculator.Calculate(TotalProfit, employees, distributionSettings.Value);
    }
}
