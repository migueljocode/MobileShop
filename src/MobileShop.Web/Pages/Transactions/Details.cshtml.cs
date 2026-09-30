namespace MobileShop.Web.Pages.Transactions;

public class DetailsModel(
    ITransactionsDataService dataService) : PageModel
{
    public TransactionDetailsViewModel? Transaction { get; private set; }

    /// <summary>The identifier of the transaction being displayed.</summary>
    public int Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Id = id;
        Transaction = await dataService.GetDetailsAsync(id);
        return Transaction is null ? NotFound() : Page();
    }

    /// <summary>Returns the factor for exactly this transaction as a browser-printable PDF.</summary>
    public async Task<IActionResult> OnGetFactorAsync(int id)
    {
        var pdfBytes = await dataService.GetTransactionFactorPdfAsync(id);
        if (pdfBytes is null)
            return NotFound();

        return File(pdfBytes, "application/pdf");
    }
}
