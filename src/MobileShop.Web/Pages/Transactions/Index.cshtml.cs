namespace MobileShop.Web.Pages.Transactions;

public class IndexModel(ITransactionDataService transactionDataService) : PageModel
{
    public IReadOnlyList<Transaction> Transactions { get; private set; } = [];
    public string Direction { get; private set; } = "all";
    public int Take { get; private set; } = 50;
    public string Order { get; private set; } = "desc";

    public void OnGet(string? direction = null, int take = 50, string? order = null)
    {
        Direction = string.IsNullOrWhiteSpace(direction) ? "all" : direction.ToLowerInvariant();
        Order = order?.ToLowerInvariant() == "asc" ? "asc" : "desc";
        Take = Math.Clamp(take, 1, 500);
        var query = transactionDataService.GetAll();
        if (Direction == "buy") query = query.Where(t => t.Direction == TransactionDirection.Buy);
        if (Direction == "sell") query = query.Where(t => t.Direction == TransactionDirection.Sell);
        Transactions = (Order == "asc"
                ? query.OrderBy(t => t.Date)
                : query.OrderByDescending(t => t.Date))
            .Take(Take).ToList();
    }
}
