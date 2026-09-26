using Microsoft.Extensions.Options;
using MobileShop.Services.Logging.Settings;
using MobileShop.Services.DataServices.Interfaces;
using MobileShop.Web.Pages.Reports;
using Moq;
using Xunit;

namespace MobileShop.Tests.Web.Pages.Reports;

/// <summary>
/// Verifies the Profit/Loss page model date-range resolution:
/// - All four automatic presets compute exact bounds.
/// - Default mode/preset is Automatic/Month.
/// - Manual mode defaults missing From/To independently.
/// - Supplied manual dates are preserved.
/// - Empty database triggers a user-facing note and falls back to today.
/// - Both profit/loss rows and total use identical effective bounds.
/// </summary>
public class ProfitLossTests
{
    private static ProfitLossModel CreateModel(
        Mock<ITransactionDataService> serviceMock,
        Mock<IEmployeeDataService>? employeeMock = null)
    {
        employeeMock ??= new Mock<IEmployeeDataService>();
        employeeMock.Setup(e => e.GetActiveEmployeesAsync())
            .ReturnsAsync([]);

        var options = Options.Create(new DistributionSettings());

        return new ProfitLossModel(
            serviceMock.Object,
            employeeMock.Object,
            options);
    }

    private static void SetupProfitLossMocks(Mock<ITransactionDataService> serviceMock)
    {
        serviceMock.Setup(s => s.GetProfitLossRowsAsync(
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync([]);

        serviceMock.Setup(s => s.GetProfitLossTotalAsync(
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(0m);
    }

    // ── Automatic presets ──────────────────────────────────────

    [Fact]
    public async Task Automatic_Today_preset_resolves_to_today_through_today()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        var model = CreateModel(mock);

        model.Mode = DateRangeMode.Automatic;
        model.Preset = AutomaticPreset.Today;

        await model.OnGetAsync();

        var today = DateTime.Today;
        Assert.Equal(today, model.EffectiveFrom);
        Assert.Equal(today, model.EffectiveTo);
        Assert.Null(model.EmptyDatabaseNote);
    }

    [Fact]
    public async Task Automatic_Week_preset_resolves_to_monday_through_today()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        var model = CreateModel(mock);

        model.Mode = DateRangeMode.Automatic;
        model.Preset = AutomaticPreset.Week;

        await model.OnGetAsync();

        var today = DateTime.Today;
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        var expectedMonday = today.AddDays(-daysSinceMonday);

        Assert.Equal(expectedMonday, model.EffectiveFrom);
        Assert.Equal(today, model.EffectiveTo);
        Assert.Null(model.EmptyDatabaseNote);
    }

    [Fact]
    public async Task Automatic_Month_preset_resolves_to_first_of_month_through_today()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        var model = CreateModel(mock);

        model.Mode = DateRangeMode.Automatic;
        model.Preset = AutomaticPreset.Month;

        await model.OnGetAsync();

        var today = DateTime.Today;
        var expectedFirstOfMonth = new DateTime(today.Year, today.Month, 1);

        Assert.Equal(expectedFirstOfMonth, model.EffectiveFrom);
        Assert.Equal(today, model.EffectiveTo);
        Assert.Null(model.EmptyDatabaseNote);
    }

    [Fact]
    public async Task Automatic_Year_preset_resolves_to_jan1_through_today()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        var model = CreateModel(mock);

        model.Mode = DateRangeMode.Automatic;
        model.Preset = AutomaticPreset.Year;

        await model.OnGetAsync();

        var today = DateTime.Today;
        var expectedJan1 = new DateTime(today.Year, 1, 1);

        Assert.Equal(expectedJan1, model.EffectiveFrom);
        Assert.Equal(today, model.EffectiveTo);
        Assert.Null(model.EmptyDatabaseNote);
    }

    [Fact]
    public async Task Default_mode_is_Automatic_and_preset_is_Month()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        var model = CreateModel(mock);

        await model.OnGetAsync();

        Assert.Equal(DateRangeMode.Automatic, model.Mode);
        Assert.Equal(AutomaticPreset.Month, model.Preset);

        var today = DateTime.Today;
        Assert.Equal(new DateTime(today.Year, today.Month, 1), model.EffectiveFrom);
        Assert.Equal(today, model.EffectiveTo);
    }

    // ── Manual mode ────────────────────────────────────────────

    [Fact]
    public async Task Manual_missing_From_defaults_to_earliest_transaction_date()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        var earliestTx = new DateTime(2025, 3, 10);
        mock.Setup(s => s.GetEarliestTransactionDateAsync())
            .ReturnsAsync(earliestTx);

        var model = CreateModel(mock);
        model.Mode = DateRangeMode.Manual;
        model.From = null;
        model.To = new DateTime(2026, 1, 15);

        await model.OnGetAsync();

        Assert.Equal(earliestTx, model.EffectiveFrom);
        Assert.Equal(new DateTime(2026, 1, 15), model.EffectiveTo);
        // From property written back so date picker shows the default
        Assert.Equal(earliestTx, model.From);
    }

    [Fact]
    public async Task Manual_missing_To_defaults_to_today()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        var earliestTx = new DateTime(2024, 7, 1);
        mock.Setup(s => s.GetEarliestTransactionDateAsync())
            .ReturnsAsync(earliestTx);

        var model = CreateModel(mock);
        model.Mode = DateRangeMode.Manual;
        model.From = new DateTime(2025, 1, 1);
        model.To = null;

        await model.OnGetAsync();

        Assert.Equal(new DateTime(2025, 1, 1), model.EffectiveFrom);
        Assert.Equal(DateTime.Today, model.EffectiveTo);
        Assert.Equal(DateTime.Today, model.To);
    }

    [Fact]
    public async Task Manual_both_missing_default_to_earliest_and_today()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        var earliestTx = new DateTime(2023, 5, 20);
        mock.Setup(s => s.GetEarliestTransactionDateAsync())
            .ReturnsAsync(earliestTx);

        var model = CreateModel(mock);
        model.Mode = DateRangeMode.Manual;
        model.From = null;
        model.To = null;

        await model.OnGetAsync();

        Assert.Equal(earliestTx, model.EffectiveFrom);
        Assert.Equal(DateTime.Today, model.EffectiveTo);
        Assert.Equal(earliestTx, model.From);
        Assert.Equal(DateTime.Today, model.To);
    }

    [Fact]
    public async Task Manual_supplied_From_preserved_when_To_missing()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        mock.Setup(s => s.GetEarliestTransactionDateAsync())
            .ReturnsAsync(new DateTime(2020, 1, 1));

        var model = CreateModel(mock);
        model.Mode = DateRangeMode.Manual;
        model.From = new DateTime(2025, 6, 1);
        model.To = null;

        await model.OnGetAsync();

        Assert.Equal(new DateTime(2025, 6, 1), model.EffectiveFrom);
        Assert.Equal(new DateTime(2025, 6, 1), model.From);
        Assert.Equal(DateTime.Today, model.EffectiveTo);
        Assert.Equal(DateTime.Today, model.To);
    }

    [Fact]
    public async Task Manual_supplied_To_preserved_when_From_missing()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        mock.Setup(s => s.GetEarliestTransactionDateAsync())
            .ReturnsAsync(new DateTime(2020, 1, 1));

        var model = CreateModel(mock);
        model.Mode = DateRangeMode.Manual;
        model.From = null;
        model.To = new DateTime(2026, 12, 31);

        await model.OnGetAsync();

        Assert.Equal(new DateTime(2026, 12, 31), model.EffectiveTo);
        Assert.Equal(new DateTime(2026, 12, 31), model.To);
        Assert.Equal(new DateTime(2020, 1, 1), model.EffectiveFrom);
        Assert.Equal(new DateTime(2020, 1, 1), model.From);
    }

    [Fact]
    public async Task Manual_both_supplied_preserved_independently()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        mock.Setup(s => s.GetEarliestTransactionDateAsync())
            .ReturnsAsync(new DateTime(2019, 1, 1));

        var model = CreateModel(mock);
        model.Mode = DateRangeMode.Manual;
        model.From = new DateTime(2025, 3, 15);
        model.To = new DateTime(2026, 9, 20);

        await model.OnGetAsync();

        Assert.Equal(new DateTime(2025, 3, 15), model.EffectiveFrom);
        Assert.Equal(new DateTime(2026, 9, 20), model.EffectiveTo);
        Assert.Equal(new DateTime(2025, 3, 15), model.From);
        Assert.Equal(new DateTime(2026, 9, 20), model.To);
        Assert.Null(model.EmptyDatabaseNote);
    }

    // ── Empty database ─────────────────────────────────────────

    [Fact]
    public async Task Empty_database_defaults_From_to_today_shows_note()
    {
        var mock = new Mock<ITransactionDataService>();
        SetupProfitLossMocks(mock);
        mock.Setup(s => s.GetEarliestTransactionDateAsync())
            .ReturnsAsync((DateTime?)null); // empty

        var model = CreateModel(mock);
        model.Mode = DateRangeMode.Manual;
        model.From = null;
        model.To = null;

        await model.OnGetAsync();

        Assert.Equal(DateTime.Today, model.EffectiveFrom);
        Assert.Equal(DateTime.Today, model.EffectiveTo);
        Assert.Equal(DateTime.Today, model.From);
        Assert.Equal(DateTime.Today, model.To);
        Assert.NotNull(model.EmptyDatabaseNote);
        Assert.Contains("No transactions recorded yet", model.EmptyDatabaseNote!);
    }

    // ── Parity ─────────────────────────────────────────────────

    [Fact]
    public async Task Rows_and_total_receive_identical_effective_bounds()
    {
        DateTime? capturedRowsFrom = null, capturedRowsTo = null;
        DateTime? capturedTotalFrom = null, capturedTotalTo = null;

        var mock = new Mock<ITransactionDataService>();
        mock.Setup(s => s.GetProfitLossRowsAsync(
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .Callback<DateTime?, DateTime?>((f, t) =>
            {
                capturedRowsFrom = f;
                capturedRowsTo = t;
            })
            .ReturnsAsync([]);
        mock.Setup(s => s.GetProfitLossTotalAsync(
            It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .Callback<DateTime?, DateTime?>((f, t) =>
            {
                capturedTotalFrom = f;
                capturedTotalTo = t;
            })
            .ReturnsAsync(0m);
        mock.Setup(s => s.GetEarliestTransactionDateAsync())
            .ReturnsAsync(new DateTime(2024, 1, 1));

        var model = CreateModel(mock);
        model.Mode = DateRangeMode.Manual;
        model.From = null; // triggers default
        model.To = null;

        await model.OnGetAsync();

        Assert.Equal(capturedRowsFrom, capturedTotalFrom);
        Assert.Equal(capturedRowsTo, capturedTotalTo);
        Assert.Equal(model.EffectiveFrom, capturedRowsFrom);
        Assert.Equal(model.EffectiveTo, capturedRowsTo);
    }

    [Fact]
    public async Task Inclusive_bounds_include_transactions_on_both_From_and_To()
    {
        var mock = new Mock<ITransactionDataService>();
        var capturedRowsFrom = (DateTime?)null;
        var capturedRowsTo = (DateTime?)null;
        var capturedTotalFrom = (DateTime?)null;
        var capturedTotalTo = (DateTime?)null;

        mock.Setup(s => s.GetProfitLossRowsAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .Callback<DateTime?, DateTime?>((f, t) =>
            {
                capturedRowsFrom = f;
                capturedRowsTo = t;
            })
            .ReturnsAsync([]);
        mock.Setup(s => s.GetProfitLossTotalAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .Callback<DateTime?, DateTime?>((f, t) =>
            {
                capturedTotalFrom = f;
                capturedTotalTo = t;
            })
            .ReturnsAsync(0m);
        mock.Setup(s => s.GetEarliestTransactionDateAsync())
            .ReturnsAsync(new DateTime(2024, 1, 1));

        var model = CreateModel(mock);
        model.Mode = DateRangeMode.Manual;
        model.From = new DateTime(2025, 6, 15);
        model.To = new DateTime(2025, 6, 20);

        await model.OnGetAsync();

        Assert.Equal(new DateTime(2025, 6, 15), capturedRowsFrom);
        Assert.Equal(new DateTime(2025, 6, 20), capturedRowsTo);
        Assert.Equal(capturedRowsFrom, capturedTotalFrom);
        Assert.Equal(capturedRowsTo, capturedTotalTo);
    }
}
