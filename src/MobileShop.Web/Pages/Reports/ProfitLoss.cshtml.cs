namespace MobileShop.Web.Pages.Reports;

public class ProfitLossModel(ITransactionDataService transactionDataService) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }
    public IReadOnlyList<Row> Rows { get; private set; } = [];
    public decimal TotalProfit => Rows.Sum(row => row.Profit);

    public void OnGet()
    {
        var transactions = transactionDataService.GetAll()
            .Where(t => (!From.HasValue || t.Date.Date >= From.Value.Date) &&
                        (!To.HasValue || t.Date.Date <= To.Value.Date))
            .GroupBy(t => t.ProductId);
        Rows = transactions.Select(group =>
        {
            var bought = group.Where(t => t.Direction == TransactionDirection.Buy).Sum(t => t.FinishedPrice);
            var sold = group.Where(t => t.Direction == TransactionDirection.Sell).Sum(t => t.FinishedPrice);
            var first = group.First();
            return new Row(group.Key, first.ProductNavigation is null ? $"Product #{group.Key}" : $"{first.ProductNavigation.Manufacturer} {first.ProductNavigation.Model}", bought, sold);
        }).OrderBy(row => row.ProductId).ToList();
    }

    public sealed record Row(int ProductId, string Product, decimal Bought, decimal Sold)
    {
        public decimal Profit => Sold - Bought;
    }
}
