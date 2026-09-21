namespace MobileShop.Web.Pages.Products;

public class CreateAppleIdModel(
    IAppleIdDataService appleIdDataService,
    IManufacturerRepo manufacturerRepo,
    IModelRepo modelRepo,
    ICategoryRepo categoryRepo) : PageModel
{
    [BindProperty] public CreateAppleIdInputModel Input { get; set; } = new();
    public string? Message { get; private set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim();
        if (appleIdDataService.FindByEmail(email) is not null)
        {
            ModelState.AddModelError(nameof(Input.Email), "This Apple ID email already exists.");
            return Page();
        }

        // the catalog categories come from the seed data - never created here
        var category = categoryRepo.Find(c => c.Name == "AppleId")
            ?? throw new InvalidOperationException("The 'AppleId' category is missing from the catalog seed data.");

        var manufacturerName = Input.Manufacturer.Trim();
        var manufacturer = manufacturerRepo.Find(m => m.Name == manufacturerName)
            ?? AddManufacturer(manufacturerName);

        var modelName = Input.Model.Trim();
        var model = modelRepo.Find(m => m.ManufacturerId == manufacturer.Id && m.Name == modelName)
            ?? AddModel(manufacturer.Id, category.Id, modelName);

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

        if (!appleIdDataService.Add(appleId))
        {
            Message = "The Apple ID could not be saved.";
            return Page();
        }

        return RedirectToPage("/Products/Details", new { id = appleId.Id, type = "appleid" });
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
}
