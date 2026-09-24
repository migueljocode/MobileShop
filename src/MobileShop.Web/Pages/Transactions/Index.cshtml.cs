namespace MobileShop.Web.Pages.Transactions;

public class IndexModel(ITransactionDataService transactionDataService) : PageModel
{
    public IReadOnlyList<TransactionListItemViewModel> Transactions { get; private set; } = [];
    public string Direction { get; private set; } = "all";
    public int Take { get; private set; } = 50;
    public string Order { get; private set; } = "desc";

    public void OnGet(string? direction = null, int take = 50, string? order = null)
    {
        Direction = string.IsNullOrWhiteSpace(direction) ? "all" : direction.ToLowerInvariant();
        Order = order?.ToLowerInvariant() == "asc" ? "asc" : "desc";
        Take = Math.Clamp(take, 1, 500);
        Transactions = transactionDataService.GetList(Direction, Take, Order == "asc");
    }

    public IActionResult OnGetPrint()
    {
        var pdfBytes = transactionDataService.GenerateTransactionsPdf(Direction, Take, Order);
        return File(pdfBytes, "application/pdf", "transactions.pdf");
    }

    public IActionResult OnGetDownloadPdf()
    {
        var pdfBytes = transactionDataService.GenerateTransactionsPdf(Direction, Take, Order);
        return File(pdfBytes, "application/pdf", $"transactions-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
