namespace MobileShop.Web.Pages.Products;

public class IndexModel(
    IPhoneDataService phoneDataService,
    IAppleIdDataService appleIdDataService) : PageModel
{
    public string Type { get; private set; } = "all";
    public IReadOnlyList<ProductRow> Products { get; private set; } = [];

    public void OnGet(string? type = null)
    {
        Type = string.IsNullOrWhiteSpace(type) ? "all" : type.ToLowerInvariant();
        var rows = new List<ProductRow>();

        if (Type is "all" or "phone")
        {
            rows.AddRange(phoneDataService.GetAll().Select(phone => new ProductRow(
                phone.Id, phone.ProductId, "Phone", $"{phone.ProductNavigation.Manufacturer} {phone.ProductNavigation.Model}",
                $"IMEI: {phone.IMEI1}", phoneDataService.IsSold(phone.Id),
                phoneDataService.IsSecondHand(phone.Id))));
        }

        if (Type is "all" or "appleid")
        {
            rows.AddRange(appleIdDataService.GetAll().Select(appleId => new ProductRow(
                appleId.Id, appleId.ProductId, "Apple ID", appleId.ProductNavigation is null
                    ? appleId.Email
                    : $"{appleId.ProductNavigation.Manufacturer} {appleId.ProductNavigation.Model}",
                appleId.Email, appleIdDataService.IsSold(appleId.Id),
                appleIdDataService.GetSecondHandInfo(appleId.Id) is not null)));
        }

        Products = rows.OrderBy(row => row.ProductId).ToList();
    }

    public sealed record ProductRow(
        int EntityId,
        int ProductId,
        string Type,
        string Name,
        string Identifier,
        bool IsSold,
        bool IsSecondHand);
}
