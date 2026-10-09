namespace MobileShop.Web.Pages.Products;

public class CreateAppleIdModel(
    IProductsDataService dataService) : PageModel
{
    [BindProperty] public CreateAppleIdInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    [TempData] public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await dataService.CreateAppleIdAsync(Input);
        if (!result.Succeeded)
        {
            if (result.ErrorField is not null)
                ModelState.AddModelError($"{nameof(Input)}.{result.ErrorField}", result.Message!);
            else
                Message = result.Message;
            return Page();
        }

        SuccessMessage = "Apple ID created successfully.";
        return RedirectToPage();
    }
}
