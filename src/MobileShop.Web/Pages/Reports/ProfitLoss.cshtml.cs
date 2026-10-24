namespace MobileShop.Web.Pages.Reports;

public enum DateRangeMode { Automatic, Manual }
public enum AutomaticPreset { Today, Week, Month, Year, All }

public class ProfitLossModel(
    IReportsDataService dataService) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateRangeMode Mode { get; set; } = DateRangeMode.Automatic;
    [BindProperty(SupportsGet = true)] public AutomaticPreset Preset { get; set; } = AutomaticPreset.Today;

    [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }

    public DateTime EffectiveFrom { get; private set; }
    public DateTime EffectiveTo { get; private set; }
    public string? EmptyDatabaseNote { get; private set; }

    public IReadOnlyList<ProfitLossRowViewModel> Rows { get; private set; } = [];
    public IReadOnlyList<ProfitLossTrendPoint> TrendPoints { get; private set; } = [];
    public ProfitLossInterval TrendInterval { get; private set; }
    public long TotalProfit { get; private set; }

    // Distribution
    public IReadOnlyList<DistributionRow> DistributionRows { get; private set; } = [];
    public IReadOnlyList<EmployeeShareViewModel> EditableShares { get; private set; } = [];

    // Posted percents from a rejected save (employee id → percent). EditableShares keeps the stored baseline.
    public IReadOnlyDictionary<int, int> PostedPercents { get; private set; } = new Dictionary<int, int>();
    public string? DistributionError { get; private set; }
    public string? SharesError { get; private set; }

    [BindProperty] public List<EmployeeShareInput> Shares { get; set; } = [];

    [TempData] public string? SuccessMessage { get; set; }

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
                _ => (today, today)
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
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostSaveSharesAsync()
    {
        var result = await dataService.UpdateEmployeeSharesAsync(Shares);

        if (result.Success)
        {
            SuccessMessage = "Shares saved.";
            // Dates travel as Jalali strings, which the global binder parses back on the next GET.
            return RedirectToPage(
                "/Reports/ProfitLoss",
                null,
                new
                {
                    Mode,
                    Preset,
                    From = From?.ToJalaliDateString(),
                    To = To?.ToJalaliDateString(),
                },
                "distribution");
        }

        SharesError = result.Error;
        await ResolveBoundsAsync();
        await LoadAsync();
        ShowPostedShares();
        return Page();
    }

    private async Task LoadAsync()
    {
        Rows = await dataService.GetProfitLossRowsAsync(EffectiveFrom, EffectiveTo);
        TrendInterval = ResolveTrendInterval();
        TrendPoints = await dataService.GetProfitLossTrendAsync(EffectiveFrom, EffectiveTo, TrendInterval) ?? [];
        TotalProfit = await dataService.GetProfitLossTotalAsync(EffectiveFrom, EffectiveTo);

        EditableShares = await dataService.GetActiveEmployeeSharesAsync();

        try
        {
            DistributionRows = await dataService.GetDistributionRowsAsync(TotalProfit);
        }
        catch (InvalidOperationException ex)
        {
            // Bad stored shares must not block the page: the editor stays usable so the user can repair them.
            DistributionError = ex.Message;
            DistributionRows = [];
        }
    }

    /// <summary>Records the posted percents of a rejected save; the stored shares stay as the baseline.</summary>
    private void ShowPostedShares()
    {
        PostedPercents = Shares
            .DistinctBy(share => share.EmployeeId)
            .ToDictionary(share => share.EmployeeId, share => share.SharePercent);
    }

    private ProfitLossInterval ResolveTrendInterval()
    {
        if (Mode == DateRangeMode.Automatic)
        {
            return Preset switch
            {
                AutomaticPreset.Today => ProfitLossInterval.Hour,
                AutomaticPreset.Week => ProfitLossInterval.Day,
                AutomaticPreset.Month => ProfitLossInterval.Day,
                AutomaticPreset.Year => ProfitLossInterval.Month,
                AutomaticPreset.All => ProfitLossInterval.Year,
                _ => ProfitLossInterval.Hour
            };
        }

        var rangeDays = (EffectiveTo.Date - EffectiveFrom.Date).TotalDays;
        return rangeDays switch
        {
            <= 1 => ProfitLossInterval.Hour,
            <= 14 => ProfitLossInterval.Day,
            <= 62 => ProfitLossInterval.Week,
            _ => ProfitLossInterval.Month
        };
    }
}
