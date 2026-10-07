namespace MobileShop.Web.Pages.People;

public class PeopleIndexModel(IPeopleDataService dataService) : PageModel
{
    public sealed record PersonRow(
        string Role,
        int Id,
        string Name,
        string PhoneNumber,
        string? NationalId,
        string? EntityType,
        int ActivityCount,
        string DetailsPage);

    public string Type { get; private set; } = "all";
    public string SortBy { get; private set; } = "Name";
    public bool Ascending { get; private set; } = true;
    public string ActivityColumnLabel => Type switch
    {
        "customers" => "Purchased",
        "sellers" => "Sold",
        _ => "Purchases / sales",
    };
    public bool ShowAddCustomer => Type == "customers";
    public bool ShowAddSeller => Type == "sellers";
    public IReadOnlyList<PersonRow> People { get; private set; } = [];

    public async Task OnGetAsync(string? type = null, string? sortBy = null, bool ascending = true)
    {
        Type = type?.Trim().ToLowerInvariant() switch
        {
            "customers" => "customers",
            "sellers" => "sellers",
            _ => "all",
        };
        SortBy = sortBy?.Trim().ToLowerInvariant() switch
        {
            "phone" => "Phone",
            "count" => "Count",
            _ => "Name",
        };
        Ascending = ascending;

        var people = new List<PersonRow>();
        if (Type is "all" or "customers")
        {
            var customers = await dataService.GetCustomerRowsAsync(SortBy, Ascending);
            people.AddRange(customers.Select(customer => new PersonRow(
                "Customer",
                customer.Id,
                customer.Name,
                customer.PhoneNumber,
                customer.NationalId,
                null,
                customer.PurchasedCount,
                "/People/CustomerDetails")));
        }

        if (Type is "all" or "sellers")
        {
            var sellers = await dataService.GetSellerRowsAsync(SortBy, Ascending);
            people.AddRange(sellers.Select(seller => new PersonRow(
                "Seller",
                seller.Id,
                seller.Name,
                seller.PhoneNumber,
                null,
                seller.EntityType,
                seller.SoldCount,
                "/People/SellerDetails")));
        }

        Func<PersonRow, object> key = SortBy switch
        {
            "Phone" => person => person.PhoneNumber,
            "Count" => person => person.ActivityCount,
            _ => person => person.Name,
        };
        var ordered = Ascending
            ? people.OrderBy(key, Comparer<object>.Create(CompareSortValues))
            : people.OrderByDescending(key, Comparer<object>.Create(CompareSortValues));
        People = ordered.ThenBy(person => person.Role).ThenBy(person => person.Id).ToList().AsReadOnly();
    }

    private static int CompareSortValues(object left, object right)
    {
        if (left is int leftCount && right is int rightCount)
            return leftCount.CompareTo(rightCount);
        return StringComparer.OrdinalIgnoreCase.Compare(left.ToString(), right.ToString());
    }
}
