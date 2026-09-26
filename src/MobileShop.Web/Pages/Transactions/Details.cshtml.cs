namespace MobileShop.Web.Pages.Transactions;

public class DetailsModel(
    ITransactionDataService transactionDataService,
    IPdfGenerator pdfGenerator) : PageModel
{
    public TransactionDetailsViewModel? Transaction { get; private set; }

    /// <summary>The identifier of the transaction being displayed.</summary>
    public int Id { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Id = id;
        Transaction = await transactionDataService.GetDetailsAsync(id);
        return Transaction is null ? NotFound() : Page();
    }

    /// <summary>Returns the factor for exactly this transaction as a browser-printable PDF.</summary>
    public async Task<IActionResult> OnGetFactorAsync(int id)
    {
        var details = await transactionDataService.GetDetailsAsync(id);
        if (details is null)
            return NotFound();

        var model = new TransactionFactorViewModel([details.ToFactorRow(id)], DateTime.UtcNow);
        var pdfBytes = pdfGenerator.GenerateTransactionFactor(model);

        return File(pdfBytes, "application/pdf");
    }
}
