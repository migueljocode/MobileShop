namespace MobileShop.Services.DataServices.Interfaces;

/// <summary>Defines transaction, invoice, and PDF operations provided by the Transactions area.</summary>
public interface ITransactionsDataService
{
    /// <summary>Gets transaction list rows.</summary>
    /// <param name="direction">The optional transaction direction.</param>
    /// <param name="take">The maximum number of rows to return.</param>
    /// <param name="ascending">Whether to sort ascending.</param>
    Task<IReadOnlyList<TransactionListItemViewModel>> GetListAsync(string? direction, int take, bool ascending);

    /// <summary>Gets transaction details.</summary>
    /// <param name="id">The transaction identifier.</param>
    Task<TransactionDetailsViewModel?> GetDetailsAsync(int id);

    /// <summary>Gets the transaction factor PDF bytes.</summary>
    /// <param name="transactionId">The transaction identifier.</param>
    Task<byte[]> GetTransactionFactorPdfAsync(int transactionId);

    /// <summary>Gets seller party options.</summary>
    Task<IReadOnlyList<PartyOptionViewModel>> GetSellersAsync();

    /// <summary>Gets customer party options.</summary>
    Task<IReadOnlyList<PartyOptionViewModel>> GetCustomersAsync();

    /// <summary>Gets products selectable for a transaction direction.</summary>
    /// <param name="direction">The transaction direction.</param>
    Task<IReadOnlyList<ProductListItemViewModel>> GetSelectableProductsAsync(TransactionDirection direction);

    /// <summary>Records a purchase transaction.</summary>
    /// <param name="input">The purchase input.</param>
    Task<ServiceResult> RecordBuyAsync(MobileShop.Models.ViewModels.Web.BindModels.BuyInputModel input);

    /// <summary>Records a sale transaction.</summary>
    /// <param name="input">The sale input.</param>
    Task<ServiceResult> RecordSellAsync(MobileShop.Models.ViewModels.Web.BindModels.SellInputModel input);

    /// <summary>Gets an invoice for a transaction.</summary>
    /// <param name="transactionId">The transaction identifier.</param>
    Task<InvoiceViewModel?> GetInvoiceAsync(int transactionId);

    /// <summary>Generates invoice PDF bytes for a transaction.</summary>
    /// <param name="transactionId">The transaction identifier.</param>
    Task<byte[]> GenerateInvoicePdfAsync(int transactionId);
}
