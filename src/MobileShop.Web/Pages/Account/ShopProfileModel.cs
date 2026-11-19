namespace MobileShop.Web.Pages.Account;

public class ShopProfileModel(IShopProfileDataService dataService) : PageModel
{
    [BindProperty]
    public ShopProfileInput Input { get; set; } = new();

    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        var profile = await dataService.GetAsync();
        Input = new ShopProfileInput
        {
            Name = profile.Name,
            Address = profile.Address,
            Phone = profile.Phone,
            Instagram = profile.Instagram
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await dataService.SaveAsync(Input);
        if (!result.Succeeded)
        {
            var fieldKey = string.IsNullOrWhiteSpace(result.ErrorField)
                ? string.Empty
                : $"{nameof(Input)}.{result.ErrorField}";
            ModelState.AddModelError(fieldKey, result.Message ?? "The shop profile could not be saved.");
            return Page();
        }

        Message = result.Message;
        return Page();
    }
}
