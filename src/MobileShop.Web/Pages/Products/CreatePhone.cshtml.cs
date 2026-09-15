namespace MobileShop.Web.Pages.Products;

public class CreatePhoneModel(IPhoneDataService phoneDataService) : PageModel
{
    [BindProperty] public CreatePhoneInputModel Input { get; set; } = new();
    public string? Message { get; private set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var imei1 = Input.IMEI1.Trim();
        if (phoneDataService.ImeiExists(imei1))
        {
            ModelState.AddModelError(nameof(Input.IMEI1), "A phone with this IMEI already exists.");
            return Page();
        }

        var product = new Product
        {
            Manufacturer = Input.Manufacturer.Trim(),
            Model = Input.Model.Trim(),
            Price = Input.Price,
            SecondHandProfile = Input.IsSecondHand ? new SecondHand
            {
                TestPeriodDays = Input.TestPeriodDays ?? 30,
                UsedDurationDays = 0,
            } : null,
            GuaranteeProfile = Input.HasGuarantee ? new Guarantee
            {
                StartDate = DateTime.Today,
                ExpirationDate = Input.GuaranteeExpiry ?? DateTime.Today.AddYears(1),
                Corporation = string.IsNullOrWhiteSpace(Input.GuaranteeCorporation) ? "Shop Warranty" : Input.GuaranteeCorporation.Trim(),
            } : null,
        };

        var phone = new Phone
        {
            IMEI1 = imei1,
            IMEI2 = string.IsNullOrWhiteSpace(Input.IMEI2) ? null : Input.IMEI2.Trim(),
            Color = string.IsNullOrWhiteSpace(Input.Color) ? null : Input.Color.Trim(),
            OwnershipTransferred = false,
            ProductNavigation = product,
        };

        if (!phoneDataService.Add(phone))
        {
            Message = "The phone could not be saved. Check the details and try again.";
            return Page();
        }

        return RedirectToPage("/Products/Details", new { id = phone.Id, type = "phone" });
    }

}
