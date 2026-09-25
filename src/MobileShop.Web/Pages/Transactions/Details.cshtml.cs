namespace MobileShop.Web.Pages.Transactions;

public class DetailsModel(
    ITransactionDataService transactionDataService,
    IPdfGenerator pdfGenerator) : PageModel
{
    public TransactionDetailsViewModel? Transaction { get; private set; }

    /// <summary>The identifier of the transaction being displayed.</summary>
    public int Id { get; private set; }

    public IActionResult OnGet(int id)
    {
        Id = id;
        Transaction = transactionDataService.GetDetails(id);
        return Transaction is null ? NotFound() : Page();
    }

    /// <summary>Returns the factor for exactly this transaction as a browser-printable PDF.</summary>
    public IActionResult OnGetFactor(int id)
    {
        var details = transactionDataService.GetDetails(id);
        if (details is null)
            return NotFound();

        var model = new TransactionFactorViewModel([details.ToFactorRow(id)], DateTime.UtcNow);
        var pdfBytes = pdfGenerator.GenerateTransactionFactor(model);

        return File(pdfBytes, "application/pdf");
    }
}
