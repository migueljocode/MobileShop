namespace MobileShop.Models.Extensions;

/// <summary>
/// Maps transaction projections into the rows of a factor/report. The party shown for a row is the
/// customer on a sale and the seller on a purchase, because that is who the factor is written to.
/// </summary>
public static class TransactionFactorExtensions
{
    /// <summary>Maps a transaction list row into a factor row.</summary>
    /// <param name="transaction">The transaction list row.</param>
    /// <returns>The factor row.</returns>
    public static TransactionFactorRowViewModel ToFactorRow(this TransactionListItemViewModel transaction)
        => new(
            transaction.Id,
            transaction.Date,
            transaction.Direction,
            transaction.ProductLabel,
            transaction.FinishedPrice,
            PersonRole(transaction.Direction),
            PersonLabel(transaction.Direction, transaction.SellerLabel, transaction.CustomerLabel));

    /// <summary>Maps transaction details into a factor row.</summary>
    /// <param name="transaction">The transaction details, which do not carry their own identifier.</param>
    /// <param name="transactionId">The identifier of the transaction the details came from.</param>
    /// <returns>The factor row.</returns>
    public static TransactionFactorRowViewModel ToFactorRow(this TransactionDetailsViewModel transaction, int transactionId)
        => new(
            transactionId,
            transaction.Date,
            transaction.Direction,
            transaction.ProductLabel,
            transaction.FinishedPrice,
            PersonRole(transaction.Direction),
            PersonLabel(transaction.Direction, transaction.SellerLabel, transaction.CustomerLabel));

    /// <summary>Gets the party that matters for a transaction direction.</summary>
    /// <param name="direction">The transaction direction.</param>
    /// <returns><c>Customer</c> for a sale, otherwise <c>Seller</c>.</returns>
    public static string PersonRole(TransactionDirection direction)
        => direction == TransactionDirection.Sell ? "Customer" : "Seller";

    private static string PersonLabel(TransactionDirection direction, string sellerLabel, string customerLabel)
        => direction == TransactionDirection.Sell ? customerLabel : sellerLabel;
}