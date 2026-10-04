namespace MobileShop.Services.DataServices.Shared;

/// <summary>Provides the product-transaction projection shared by the Products and Transactions areas.</summary>
public static class ProductTransactionProjection
{
    /// <summary>Gets the product-transaction projection expression.</summary>
    public static Expression<Func<Transaction, ProductTransactionViewModel>> Selector =>
        transaction => new ProductTransactionViewModel(
            transaction.Date,
            transaction.Direction,
            transaction.FinishedPrice,
            transaction.SellerNavigation.PersonNavigation == null
                ? "Shop"
                : transaction.SellerNavigation.PersonNavigation.FirstName + " " + transaction.SellerNavigation.PersonNavigation.LastName,
            transaction.CustomerNavigation.PersonNavigation == null
                ? "Shop"
                : transaction.CustomerNavigation.PersonNavigation.FirstName + " " + transaction.CustomerNavigation.PersonNavigation.LastName);
}
