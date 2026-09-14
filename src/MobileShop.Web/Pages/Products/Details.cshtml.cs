namespace MobileShop.Web.Pages.Products;

public class DetailsModel(
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService,
    ITransactionDataService transactionDataService) : PageModel
{
    public ProductDetails? Product { get; private set; }
    public IReadOnlyList<Transaction> Transactions { get; private set; } = [];

    public IActionResult OnGet(int id, string? type = null)
    {
        if (string.Equals(type, "appleid", StringComparison.OrdinalIgnoreCase))
        {
            var appleId = appleIdDataService.Find(id);
            if (appleId is null) return NotFound();
            Product = new ProductDetails("Apple ID", appleId.ProductId,
                appleId.ProductNavigation?.Manufacturer ?? "Apple",
                appleId.ProductNavigation?.Model ?? appleId.Email,
                appleId.Email, appleIdDataService.GetOwner(id)?.PersonNavigation,
                appleIdDataService.GetGuarantee(id), appleIdDataService.GetSecondHandInfo(id));
        }
        else
        {
            var phone = phoneDataService.Find(id);
            if (phone is null) return NotFound();
            Product = new ProductDetails("Phone", phone.ProductId,
                phone.ProductNavigation?.Manufacturer ?? "Phone",
                phone.ProductNavigation?.Model ?? $"Phone #{id}",
                $"IMEI: {phone.IMEI1}" + (phone.IMEI2 is null ? string.Empty : $" / {phone.IMEI2}"),
                phoneDataService.GetOwner(id)?.PersonNavigation,
                phoneDataService.GetGuarantee(id), phoneDataService.GetSecondHandInfo(id));
        }

        Transactions = transactionDataService.GetByProduct(Product.ProductId).ToList();
        return Page();
    }

    public sealed record ProductDetails(
        string Type,
        int ProductId,
        string Manufacturer,
        string Model,
        string Identifier,
        Person? Owner,
        Guarantee? Guarantee,
        SecondHand? SecondHand);
}
