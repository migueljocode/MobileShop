namespace MobileShop.Web.Pages.Products;

public class CreateAppleIdModel(
    IAppleIdDataService appleIdDataService,
    IManufacturerRepo manufacturerRepo,
    IModelRepo modelRepo,
    ICategoryRepo categoryRepo) : PageModel
{
    // Apple IDs are always for the same implicit Apple iPhone model, so the user never picks one.
    private const string AppleManufacturerName = "Apple";
    private const string ImplicitAppleIdModelName = "iPhone";
    private const string AppleIdCategoryName = "AppleId";

    [BindProperty] public CreateAppleIdInputModel Input { get; set; } = new();
    public string? Message { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim();
        if (await appleIdDataService.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError(nameof(Input.Email), "This Apple ID email already exists.");
            return Page();
        }

        // Apple IDs are all manufactured by Apple
        var manufacturer = await manufacturerRepo.FindAsync(m => m.Name == AppleManufacturerName)
            ?? new Manufacturer { Name = AppleManufacturerName };

        if (manufacturer.Id == 0)
        {
            await manufacturerRepo.AddAsync(manufacturer);
        }

        // the catalog categories come from the seed data - never created here
        var category = await categoryRepo.FindAsync(c => c.Name == AppleIdCategoryName)
            ?? throw new InvalidOperationException("The 'AppleId' category is missing from the catalog seed data.");

        // Apple IDs always hang off one shared implicit Apple iPhone model inside the AppleId category
        var model = await modelRepo.FindAsync(m =>
                m.ManufacturerId == manufacturer.Id &&
                m.CategoryId == category.Id &&
                m.Name == ImplicitAppleIdModelName)
            ?? await AddModelAsync(manufacturer.Id, category.Id, ImplicitAppleIdModelName);

        var product = new Product
        {
            ModelId = model.Id,
            Barcode = Guid.NewGuid().ToString("N")[..12],
            Price = Input.Price,
        };

        var appleId = new AppleId
        {
            Email = email,
            Password = Input.Password.Trim(),
            Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim(),
            ProductNavigation = product,
        };

        var ok = await appleIdDataService.AddAsync(appleId);
        if (!ok)
        {
            Message = "The Apple ID could not be saved.";
            return Page();
        }

        return RedirectToPage("/Products/Details", new { id = appleId.Id, type = "appleid" });
    }

    private async Task<Model> AddModelAsync(int manufacturerId, int categoryId, string name)
    {
        var model = new Model { ManufacturerId = manufacturerId, CategoryId = categoryId, Name = name };
        await modelRepo.AddAsync(model);
        return model;
    }
}
