namespace MobileShop.Models.ViewModels;

/// <summary>Transaction data required to render an invoice.</summary>
public sealed record InvoiceViewModel(
    string? BuyerName,
    string? BuyerNationalId,
    string? BuyerPhoneNumber,
    string? SellerName,
    string? SellerPhoneNumber,
    DateTime TransactionDate,
    decimal FinishedPrice,
    int ProductCount,
    string ProductInformation,
    IEnumerable<(string Label, string Value)> ProductExtras,
    IEnumerable<(string Label, string Value)> GuaranteeInformation,
    bool? OwnershipTransferred,
    string? Notes);
