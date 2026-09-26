namespace MobileShop.Web.Pages.Products;

public class CreatePhoneModel(
    IPhoneDataService phoneDataService,
    IManufacturerRepo manufacturerRepo,
    IModelRepo modelRepo,
    ICategoryRepo categoryRepo,
    IColorRepo colorRepo) : PageModel
{
    [BindProperty] public CreatePhoneInputModel Input { get; set; } = new();
    public string? Message { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var imei1 = Input.IMEI1.Trim();
        if (await phoneDataService.ImeiExistsAsync(imei1))
        {
            ModelState.AddModelError(nameof(Input.IMEI1), "A phone with this IMEI already exists.");
            return Page();
        }

        // the catalog categories come from the seed data - never created here
        var category = await categoryRepo.FindAsync(c => c.Name == "Phone")
            ?? throw new InvalidOperationException("The 'Phone' category is missing from the catalog seed data.");

        var manufacturerName = Input.Manufacturer.Trim();
        var manufacturer = await manufacturerRepo.FindAsync(m => m.Name == manufacturerName)
            ?? await AddManufacturerAsync(manufacturerName);

        var modelName = Input.Model.Trim();
        var model = await modelRepo.FindAsync(m => m.ManufacturerId == manufacturer.Id && m.Name == modelName)
            ?? await AddModelAsync(manufacturer.Id, category.Id, modelName);

        var colorName = Input.Color?.Trim();
        var color = string.IsNullOrWhiteSpace(colorName)
            ? null
            : await colorRepo.FindAsync(c => c.Name == colorName) ?? await AddColorAsync(colorName);

        var product = new Product
        {
            ModelId = model.Id,
            ColorId = color?.Id,
            Barcode = Guid.NewGuid().ToString("N")[..12],
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
            OwnershipTransferred = false,
            ProductNavigation = product,
        };

        var ok = await phoneDataService.AddAsync(phone);
        if (!ok)
        {
            Message = "The phone could not be saved. Check the details and try again.";
            return Page();
        }

        return RedirectToPage("/Products/Details", new { id = phone.Id, type = "phone" });
    }

    private async Task<Manufacturer> AddManufacturerAsync(string name)
    {
        var manufacturer = new Manufacturer { Name = name };
        await manufacturerRepo.AddAsync(manufacturer);
        return manufacturer;
    }

    private async Task<Model> AddModelAsync(int manufacturerId, int categoryId, string name)
    {
        var model = new Model { ManufacturerId = manufacturerId, CategoryId = categoryId, Name = name };
        await modelRepo.AddAsync(model);
        return model;
    }

    private async Task<Color> AddColorAsync(string name)
    {
        var color = new Color { Name = name };
        await colorRepo.AddAsync(color);
        return color;
    }
}
