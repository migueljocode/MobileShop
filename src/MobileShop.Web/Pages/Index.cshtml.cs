namespace MobileShop.Web.Pages;

[Authorize]
public class IndexModel(
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService,
    ITransactionDataService transactionDataService) : PageModel
{
    public int PhonesInStock { get; private set; }
    public int PhonesSecondHand { get; private set; }
    public int PhonesSecondHandAvailable { get; private set; }
    public int AppleIdsInStock { get; private set; }

    public IReadOnlyList<PhoneRow> Phones { get; private set; } = [];
    public IReadOnlyList<TransactionRow> RecentTransactions { get; private set; } = [];

    public void OnGet()
    {
        PhonesInStock = phoneDataService.Quantity();
        PhonesSecondHand = phoneDataService.SecondHandQuantity();
        PhonesSecondHandAvailable = phoneDataService.AvailableSecondHandQuantity();
        AppleIdsInStock = appleIdDataService.Quantity();

        Phones = phoneDataService.GetAll()
            .OrderByDescending(phone => phone.Id)
            .Select(phone => new PhoneRow(
                phone.Id,
                phone.IMEI1,
                phone.IMEI2,
                phone.OwnershipTransferred,
                phoneDataService.IsSold(phone.Id),
                phoneDataService.IsSecondHand(phone.Id)))
            .ToList();

        RecentTransactions = transactionDataService.GetRecent()
            .Select(transaction => new TransactionRow(
                transaction.Id,
                transaction.Date,
                transaction.Direction,
                transaction.FinishedPrice,
                transaction.ProductId,
                transaction.SellerId,
                transaction.CustomerId))
            .ToList();
    }

    public sealed record PhoneRow(
        int Id,
        string Imei1,
        string? Imei2,
        bool OwnershipTransferred,
        bool IsSold,
        bool IsSecondHand);

    public sealed record TransactionRow(
        int Id,
        DateTime Date,
        TransactionDirection Direction,
        decimal FinishedPrice,
        int ProductId,
        int SellerId,
        int CustomerId);
}
