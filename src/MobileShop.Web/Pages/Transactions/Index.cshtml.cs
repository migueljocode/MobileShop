namespace MobileShop.Web.Pages.Transactions;

public class IndexModel(ITransactionDataService transactionDataService) : PageModel
{
    public IReadOnlyList<TransactionListItemViewModel> Transactions { get; private set; } = [];
    public string Direction { get; private set; } = "all";
    public int Take { get; private set; } = 50;
    public string Order { get; private set; } = "desc";

    /// <summary>
    /// Transactions ticked on the list. When empty the factor falls back to the current
    /// direction/count/order filters, which keeps the pre-existing filtered report working.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int[] SelectedIds { get; set; } = [];

    /// <summary>
    /// When true, the page renders the printable factor HTML and triggers the browser's
    /// native print dialog instead of the normal transaction list UI.
    /// </summary>
    public bool IsPrintMode { get; private set; }

    /// <summary>The resolved factor rows to render in print mode.</summary>
    public IReadOnlyList<TransactionFactorRowViewModel> PrintFactorRows { get; private set; } = [];

    public async Task OnGetAsync(string? direction = null, int take = 50, string? order = null)
        => await LoadAsync(direction, take, order);

    /// <summary>
    /// Returns the factor as a browser-printable HTML page. The browser's native print
    /// dialog handles printing or saving as PDF — no server-side PDF generation.
    /// </summary>
    public async Task<IActionResult> OnGetPrintFactorAsync(string? direction = null, int take = 50, string? order = null)
    {
        await LoadAsync(direction, take, order);

        var rows = ResolveFactorRows();
        if (rows.Count == 0)
        {
            // ModelState already populated with the validation error; redisplay.
            IsPrintMode = false;
            return Page();
        }

        PrintFactorRows = rows;
        IsPrintMode = true;
        return Page();
    }

    private async Task LoadAsync(string? direction, int take, string? order)
    {
        Direction = string.IsNullOrWhiteSpace(direction) ? "all" : direction.ToLowerInvariant();
        Order = string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        Take = Math.Clamp(take, 1, 500);
        Transactions = await transactionDataService.GetListAsync(Direction, Take, Order == "asc");
    }

    /// <summary>
    /// Resolves exactly the transactions the factor must contain: the manual selection when one was
    /// supplied, otherwise the filtered list. Unknown, non-positive, or unusable identifiers are reported,
    /// never silently dropped, so a partial factor is never produced.
    /// </summary>
    /// <remarks>
    /// When selected IDs are supplied, they are resolved from the in-memory <see cref="Transactions"/>
    /// snapshot loaded by <see cref="LoadAsync"/> — a single consistent database read for the entire
    /// factor. IDs absent from the snapshot (e.g. from a different page or filter) are reported as
    /// missing rather than fetched individually, preserving the single-snapshot guarantee.
    ///
    /// This means the normal case (IDs selected from the current page) requires zero extra DB
    /// round-trips, and even cross-page IDs never trigger a per-ID <see cref="ITransactionDataService.GetDetailsAsync"/>
    /// call.
    /// </remarks>
    private IReadOnlyList<TransactionFactorRowViewModel> ResolveFactorRows()
    {
        if (SelectedIds.Length == 0)
            return Transactions.Select(transaction => transaction.ToFactorRow()).ToList();

        // Any non-positive ID makes the entire selection invalid.
        if (SelectedIds.Any(id => id <= 0))
        {
            ModelState.AddModelError(string.Empty, "Selected transaction identifiers must be positive numbers.");
            return [];
        }

        var requested = SelectedIds.Distinct().ToList();

        // Build the lookup from the single snapshot loaded by LoadAsync so that
        // the normal case (IDs selected from the current page) requires zero
        // extra database round-trips and uses one consistent read.
        var snapshot = Transactions.ToDictionary(t => t.Id);

        var rows = new List<TransactionFactorRowViewModel>(requested.Count);
        var missing = new List<int>();
        foreach (var id in requested)
        {
            if (snapshot.TryGetValue(id, out var transaction))
            {
                rows.Add(transaction.ToFactorRow());
                continue;
            }

            // IDs not in the current snapshot are reported as missing rather than
            // fetched individually, preserving the single-snapshot guarantee.
            missing.Add(id);
        }

        if (missing.Count > 0)
        {
            ModelState.AddModelError(
                string.Empty,
                $"These selected transactions no longer exist: {string.Join(", ", missing)}.");
            return [];
        }

        return rows;
    }
}
