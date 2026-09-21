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

        // the catalog categories come from the seed data - never created here
        var category = categoryRepo.Find(c => c.Name == "Phone")
            ?? throw new InvalidOperationException("The 'Phone' category is missing from the catalog seed data.");

        var manufacturerName = Input.Manufacturer.Trim();
        var manufacturer = manufacturerRepo.Find(m => m.Name == manufacturerName)
            ?? AddManufacturer(manufacturerName);

        var modelName = Input.Model.Trim();
        var model = modelRepo.Find(m => m.ManufacturerId == manufacturer.Id && m.Name == modelName)
            ?? AddModel(manufacturer.Id, category.Id, modelName);

        var colorName = Input.Color?.Trim();
        var color = string.IsNullOrWhiteSpace(colorName)
            ? null
            : colorRepo.Find(c => c.Name == colorName) ?? AddColor(colorName);

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

        if (!phoneDataService.Add(phone))
        {
            Message = "The phone could not be saved. Check the details and try again.";
            return Page();
        }

        return RedirectToPage("/Products/Details", new { id = phone.Id, type = "phone" });
    }

    private Manufacturer AddManufacturer(string name)
    {
        var manufacturer = new Manufacturer { Name = name };
        manufacturerRepo.Add(manufacturer);
        return manufacturer;
    }

    private Model AddModel(int manufacturerId, int categoryId, string name)
    {
        var model = new Model { ManufacturerId = manufacturerId, CategoryId = categoryId, Name = name };
        modelRepo.Add(model);
        return model;
    }

    private Color AddColor(string name)
    {
        var color = new Color { Name = name };
        colorRepo.Add(color);
        return color;
    }
}
