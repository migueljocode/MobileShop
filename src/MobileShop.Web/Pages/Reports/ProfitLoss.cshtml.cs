namespace MobileShop.Web.Pages.Reports;

public enum DateRangeMode { Automatic, Manual }
public enum AutomaticPreset { Today, Week, Month, Year, All }

public class ProfitLossModel(
    IReportsDataService dataService) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateRangeMode Mode { get; set; } = DateRangeMode.Automatic;
    [BindProperty(SupportsGet = true)] public AutomaticPreset Preset { get; set; } = AutomaticPreset.Month;

    [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }

    public DateTime EffectiveFrom { get; private set; }
    public DateTime EffectiveTo { get; private set; }
    public string? EmptyDatabaseNote { get; private set; }

    public IReadOnlyList<ProfitLossRowViewModel> Rows { get; private set; } = [];
    public long TotalProfit { get; private set; }

    // Distribution
    public IReadOnlyList<DistributionRow> DistributionRows { get; private set; } = [];

    private async Task ResolveBoundsAsync()
    {
        var today = DateTime.Today;
        var earliest = await dataService.GetEarliestTransactionDateAsync();

        if (Mode == DateRangeMode.Automatic)
        {
            var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
            (EffectiveFrom, EffectiveTo) = Preset switch
            {
                AutomaticPreset.Today => (today, today),
                AutomaticPreset.Week => (today.AddDays(-daysSinceMonday), today),
                AutomaticPreset.Month => (new DateTime(today.Year, today.Month, 1), today),
                AutomaticPreset.Year => (new DateTime(today.Year, 1, 1), today),
                AutomaticPreset.All => (earliest ?? today, today),
                _ => (new DateTime(today.Year, today.Month, 1), today)
            };

            // Pre-populate the date pickers for initial page load; EffectiveFrom/EffectiveTo
            // stay governed by the selected preset and are never read from From/To here.
            From ??= earliest ?? today;
            To ??= today;
        }
        else
        {
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

        Rows = await dataService.GetProfitLossRowsAsync(EffectiveFrom, EffectiveTo);
        TotalProfit = await dataService.GetProfitLossTotalAsync(EffectiveFrom, EffectiveTo);

        // Distribution
        DistributionRows = await dataService.GetDistributionRowsAsync(TotalProfit);
    }
}
