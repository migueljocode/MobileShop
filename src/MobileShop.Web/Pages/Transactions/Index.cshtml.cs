namespace MobileShop.Web.Pages.Transactions;

public class IndexModel(
    ITransactionsDataService dataService) : PageModel
{
    public IReadOnlyList<TransactionListItemViewModel> Transactions { get; private set; } = [];
    public string Direction { get; private set; } = "all";
    public int Take { get; private set; } = 50;
    public string Sort { get; private set; } = "date";
    public string Order { get; private set; } = "desc";
    public int? CustomerId { get; private set; }
    public string DateRange { get; private set; } = "all";
    public IReadOnlyList<PartyOptionViewModel> Customers { get; private set; } = [];

    /// <summary>
    /// Transactions ticked on the list. When empty the factor falls back to the current
    /// direction/count/sort/order filters, which keeps the pre-existing filtered report working.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int[] SelectedIds { get; set; } = [];

    public async Task OnGetAsync(
        string? direction = null,
        int take = 50,
        string? sort = null,
        string? order = null,
        int? customerId = null,
        string? dateRange = null)
        => await LoadAsync(direction, take, sort, order, customerId, dateRange);

    /// <summary>Generates a downloadable PDF factor for the selected or filtered transactions.</summary>
    public async Task<IActionResult> OnGetDownloadFactorAsync(
        string? direction = null,
        int take = 50,
        string? sort = null,
        string? order = null,
        int? customerId = null,
        string? dateRange = null)
    {
        // The list is always loaded first so the page still renders its rows when the factor fails.
        await LoadAsync(direction, take, sort, order, customerId, dateRange);

        var (fromDate, toDate) = GetDateBounds(DateRange);
        var factor = await dataService.GenerateListFactorPdfAsync(
            Direction,
            Take,
            Order == "asc",
            SelectedIds,
            Sort,
            CustomerId,
            fromDate,
            toDate);
        if (!factor.Succeeded)
        {
            if (ModelState.IsValid)
                ModelState.AddModelError(string.Empty, factor.Error!);

            return Page();
        }

        return File(factor.Bytes!, "application/pdf", "transactions-factor.pdf");
    }

    private async Task LoadAsync(
        string? direction,
        int take,
        string? sort,
        string? order,
        int? customerId,
        string? dateRange)
    {
        Direction = string.IsNullOrWhiteSpace(direction) ? "all" : direction.ToLowerInvariant();
        Sort = sort?.Trim().ToLowerInvariant() switch
        {
            "product" => "product",
            "price" => "price",
            "seller" => "seller",
            "customer" => "customer",
            _ => "date",
        };
        Order = string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        Take = Math.Clamp(take, 1, 500);
        CustomerId = customerId is > 0 ? customerId : null;
        DateRange = dateRange?.Trim().ToLowerInvariant() switch
        {
            "today" => "today",
            "week" => "week",
            "month" => "month",
            "year" => "year",
            _ => "all",
        };
        Customers = await dataService.GetCustomersAsync();
        var (from, to) = GetDateBounds(DateRange);
        Transactions = await dataService.GetListAsync(Direction, Take, Order == "asc", Sort, CustomerId, from, to);
    }

    private static (DateTime? From, DateTime? To) GetDateBounds(string dateRange)
    {
        var today = DateTime.Today;
        return dateRange switch
        {
            "today" => (today, today),
            "week" => (today.AddDays(-(((int)today.DayOfWeek + 6) % 7)), today),
            "month" => (new DateTime(today.Year, today.Month, 1), today),
            "year" => (new DateTime(today.Year, 1, 1), today),
            _ => (null, null),
        };
    }
}
