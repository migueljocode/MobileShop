namespace MobileShop.Web.Pages.Transactions;

public class IndexModel(
    ITransactionDataService transactionDataService,
    IPdfGenerator pdfGenerator) : PageModel
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

    public async Task OnGetAsync(string? direction = null, int take = 50, string? order = null)
        => await LoadAsync(direction, take, order);

    /// <summary>Returns the factor as a browser-printable, inline PDF response.</summary>
    public async Task<IActionResult> OnGetPrintAsync(string? direction = null, int take = 50, string? order = null)
        => await GenerateFactorAsync(direction, take, order, download: false);

    /// <summary>Returns the factor as a named, downloadable PDF response.</summary>
    public async Task<IActionResult> OnGetDownloadAsync(string? direction = null, int take = 50, string? order = null)
        => await GenerateFactorAsync(direction, take, order, download: true);

    private async Task LoadAsync(string? direction, int take, string? order)
    {
        Direction = string.IsNullOrWhiteSpace(direction) ? "all" : direction.ToLowerInvariant();
        Order = string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        Take = Math.Clamp(take, 1, 500);
        Transactions = await transactionDataService.GetListAsync(Direction, Take, Order == "asc");
    }

    private async Task<IActionResult> GenerateFactorAsync(string? direction, int take, string? order, bool download)
    {
        // Keep the filters and the ticked rows on screen when the request has to be redisplayed.
        await LoadAsync(direction, take, order);

        var rows = ResolveFactorRows();
        if (rows.Count == 0)
        {
            if (!ModelState.ContainsKey(string.Empty))
                ModelState.AddModelError(string.Empty, "Select at least one transaction, or widen the filters, before generating a factor.");
            return Page();
        }

        var pdfBytes = pdfGenerator.GenerateTransactionFactor(new TransactionFactorViewModel(rows, DateTime.UtcNow));

        return download
            ? File(pdfBytes, "application/pdf", $"transactions-factor-{DateTime.Now:yyyyMMdd-HHmmss}.pdf")
            : File(pdfBytes, "application/pdf");
    }

    /// <summary>
    /// Resolves exactly the transactions the factor must contain: the manual selection when one was
    /// supplied, otherwise the filtered list. Unknown or unusable identifiers are reported, never
    /// silently dropped, so a partial factor is never produced.
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

        var requested = SelectedIds.Where(id => id > 0).Distinct().ToList();
        if (requested.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "The selection did not contain any usable transaction identifiers.");
            return [];
        }

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
