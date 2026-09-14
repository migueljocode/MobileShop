namespace MobileShop.Web.Pages.Transactions;

public class DetailsModel(ITransactionDataService transactionDataService) : PageModel
{
    public TransactionDetailsViewModel? Transaction { get; private set; }

    public IActionResult OnGet(int id)
    {
        Transaction = transactionDataService.GetDetails(id);
        return Transaction is null ? NotFound() : Page();
    }
}
