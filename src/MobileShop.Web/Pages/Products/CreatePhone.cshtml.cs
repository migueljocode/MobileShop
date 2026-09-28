namespace MobileShop.Web.Pages.Products;

public class CreatePhoneModel(
    IPhoneDataService phoneDataService,
    IManufacturerRepo manufacturerRepo,
    IModelRepo modelRepo,
    ICategoryRepo categoryRepo,
    IColorRepo colorRepo,
    IGuaranteeRepo guaranteeRepo) : PageModel
{
    [BindProperty] public CreatePhoneInputModel Input { get; set; } = new();
    public string? Message { get; private set; }
    public IEnumerable<Manufacturer> Manufacturers { get; private set; } = [];
    public IEnumerable<Model> Models { get; private set; } = [];
    public IEnumerable<Color> Colors { get; private set; } = [];
    public IEnumerable<string> Corporations { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await PopulateDropdownsAsync();
    }

    public async Task<IActionResult> OnGetModelsAsync(int manufacturerId)
    {
        var models = await modelRepo.GetByManufacturerAsync(manufacturerId);
        return new JsonResult(models.Select(m => new { m.Id, m.Name }));
    }

    public async Task<IActionResult> OnPostCreateManufacturerAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new JsonResult(new { error = "Name is required." }) { StatusCode = 400 };

        var trimmed = name.Trim();
        var existing = await manufacturerRepo.FindAsync(m => m.Name == trimmed);
        if (existing is not null)
            return new JsonResult(new { id = existing.Id, name = existing.Name });

        var manufacturer = new Manufacturer { Name = trimmed };
        await manufacturerRepo.AddAsync(manufacturer);
        return new JsonResult(new { id = manufacturer.Id, name = manufacturer.Name });
    }

    public async Task<IActionResult> OnPostCreateModelAsync(int manufacturerId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new JsonResult(new { error = "Name is required." }) { StatusCode = 400 };

        var manufacturer = await manufacturerRepo.FindAsync(manufacturerId);
        if (manufacturer is null)
            return new JsonResult(new { error = "Manufacturer not found." }) { StatusCode = 404 };

        var category = await categoryRepo.FindAsync(c => c.Name == "Phone")
            ?? throw new InvalidOperationException("The 'Phone' category is missing from the catalog seed data.");

        var trimmed = name.Trim();
        var existing = await modelRepo.FindAsync(m =>
            m.ManufacturerId == manufacturerId && m.Name == trimmed && m.CategoryId == category.Id);
        if (existing is not null)
            return new JsonResult(new { id = existing.Id, name = existing.Name });

        var model = new Model { ManufacturerId = manufacturerId, CategoryId = category.Id, Name = trimmed };
        await modelRepo.AddAsync(model);
        return new JsonResult(new { id = model.Id, name = model.Name });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return Page();
        }

        var imei1 = Input.IMEI1.Trim();
        if (await phoneDataService.ImeiExistsAsync(imei1))
        {
            ModelState.AddModelError(nameof(Input.IMEI1), "A phone with this IMEI already exists.");
            await PopulateDropdownsAsync();
            return Page();
        }

        var manufacturer = await manufacturerRepo.FindAsync(Input.ManufacturerId);
        if (manufacturer is null)
        {
            ModelState.AddModelError(nameof(Input.ManufacturerId), "Selected manufacturer not found.");
            await PopulateDropdownsAsync();
            return Page();
        }

        var model = await modelRepo.FindAsync(Input.ModelId);
        if (model is null || model.ManufacturerId != manufacturer.Id)
        {
            ModelState.AddModelError(nameof(Input.ModelId), "Selected model not found for this manufacturer.");
            await PopulateDropdownsAsync();
            return Page();
        }

        var color = Input.ColorId is null
            ? null
            : await colorRepo.FindAsync(Input.ColorId.Value) ?? null;
        if (Input.ColorId.HasValue && color is null)
        {
            ModelState.AddModelError(nameof(Input.ColorId), "Selected color not found.");
            await PopulateDropdownsAsync();
            return Page();
        }

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
            await PopulateDropdownsAsync();
            return Page();
        }

        return RedirectToPage("/Products/Details", new { id = phone.Id, type = "phone" });
    }

    public async Task<IActionResult> OnPostCreateColorAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new JsonResult(new { error = "Name is required." }) { StatusCode = 400 };

        var trimmed = name.Trim();
        var existing = await colorRepo.FindAsync(c => c.Name == trimmed);
        if (existing is not null)
            return new JsonResult(new { id = existing.Id, name = existing.Name });

        var color = new Color { Name = trimmed };
        await colorRepo.AddAsync(color);
        return new JsonResult(new { id = color.Id, name = color.Name });
    }

    public async Task<IActionResult> OnPostCreateCorporationAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new JsonResult(new { error = "Name is required." }) { StatusCode = 400 };

        var trimmed = name.Trim();
        if (!Corporations.Any())
            await PopulateDropdownsAsync();

        var existing = Corporations.FirstOrDefault(c =>
            string.Equals(c, trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
            return new JsonResult(new { name = existing });

        return new JsonResult(new { name = trimmed });
    }

    private async Task PopulateDropdownsAsync()
    {
        Manufacturers = await manufacturerRepo.FindAllAsync();
        Models = Input.ManufacturerId > 0
            ? await modelRepo.GetByManufacturerAsync(Input.ManufacturerId)
            : [];
        Colors = await colorRepo.FindAllAsync();
        Corporations = (await guaranteeRepo.FindAllAsync())
            .Select(g => g.Corporation)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.Ordinal);
    }
}
