namespace MobileShop.Web.Pages.Products;

public sealed class PersonPickerModel(IPeopleDataService peopleDataService) : PageModel
{
    public async Task<IActionResult> OnGetSearchSellersAsync(string? q) =>
        new JsonResult(await peopleDataService.SearchSellersAsync(q));

    public async Task<IActionResult> OnPostCreateSellerAsync(CreateSellerInputModel input)
    {
        ModelState.Clear();
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), validationResults, true))
            return CreateErrorResult("Please correct the seller details.");

        var result = await peopleDataService.CreateSellerAsync(input);
        if (!result.Succeeded || result.EntityId is null)
            return CreateErrorResult(result.Message ?? "The seller could not be created.");

        var name = $"{input.FirstName.Trim()} {input.LastName.Trim()}".Trim();
        return new JsonResult(new DropdownCreateResult(
            true,
            new DropdownOptionViewModel(result.EntityId.Value, name),
            null,
            200));
    }

    private static JsonResult CreateErrorResult(string message) =>
        new(new DropdownCreateResult(false, null, message, 400)) { StatusCode = 400 };
}
