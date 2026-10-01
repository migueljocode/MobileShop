using MobileShop.Services.DataServices.Shared;

namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the catalog and creation operations for the Products area.</summary>
/// <remarks>
/// The eight repositories cover both product types plus the shared catalog entities and transactions. There is no
/// <see cref="Product"/> repo because each create path builds the product row inline and it is persisted by
/// cascade through the phone or Apple-ID insert, and no <see cref="SecondHand"/> repo (the second-hand profile is
/// a <see cref="Product"/> navigation, not a separate write).
/// </remarks>
public class ProductsDataService(
    IBaseRepo<Phone> phones,
    IBaseRepo<AppleId> appleIds,
    IBaseRepo<Manufacturer> manufacturers,
    IBaseRepo<Model> models,
    IBaseRepo<Category> categories,
    IBaseRepo<MobileShop.Models.Entities.Color> colors,
    IBaseRepo<Guarantee> guarantees,
    IBaseRepo<Transaction> transactions,
    ILogger<ProductsDataService> logger)
    : IProductsDataService
{
        /// <summary>Gets the structured logger for this products service.</summary>
    protected ILogger<ProductsDataService> Logger { get; } = logger;

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync(string? type = null)
    {
        var rows = new List<ProductListItemViewModel>();

        // Only "appleid" excludes phones; only "phone" excludes Apple IDs — null, "all"
        // and any unrecognised value return both blocks. Pages lowercase before calling.
        if (type is not "appleid")
            rows.AddRange((await phones.SelectAllAsync(
                phone => new ProductListItemViewModel(
                    phone.Id,
                    phone.ProductId,
                    "Phone",
                    phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                    "IMEI: " + phone.IMEI1,
                    phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name,
                    phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    phone.ProductNavigation.SecondHandProfile != null)))
                .OrderBy(row => row.ProductId));

        if (type is not "phone")
            rows.AddRange((await appleIds.SelectAllAsync(
                appleId => new ProductListItemViewModel(
                    appleId.Id,
                    appleId.ProductId,
                    "Apple ID",
                    appleId.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + appleId.ProductNavigation.ModelNavigation.Name,
                    appleId.Email,
                    null,
                    appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    appleId.ProductNavigation.SecondHandProfile != null)))
                .OrderBy(row => row.ProductId));

        return rows.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> GetSecondHandRowsAsync()
    {
        var phoneRows = (await phones.SelectAllAsync(
            phone => phone.ProductNavigation.SecondHandProfile != null,
            phone => new ProductListItemViewModel(
                phone.Id,
                phone.ProductId,
                "Phone",
                phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                "IMEI: " + phone.IMEI1,
                phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name,
                phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                phone.ProductNavigation.SecondHandProfile != null)))
            .OrderBy(row => row.ProductId)
            .ToList();

        var appleRows = (await appleIds.SelectAllAsync(
            appleId => appleId.ProductNavigation.SecondHandProfile != null,
            appleId => new ProductListItemViewModel(
                appleId.Id,
                appleId.ProductId,
                "Apple ID",
                appleId.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + appleId.ProductNavigation.ModelNavigation.Name,
                appleId.Email,
                null,
                appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                appleId.ProductNavigation.SecondHandProfile != null)))
            .OrderBy(row => row.ProductId)
            .ToList();

        return phoneRows.Concat(appleRows).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<ProductDetailsViewModel?> GetDetailsAsync(int id, string type)
    {
        ProductDetailsViewModel? details;

        if (string.Equals(type, "appleid", StringComparison.OrdinalIgnoreCase))
        {
            details = await appleIds.SelectAsync(
                id,
                appleId => new ProductDetailsViewModel(
                    "Apple ID",
                    appleId.ProductId,
                    appleId.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name,
                    appleId.ProductNavigation.ModelNavigation.Name,
                    "Email: " + appleId.Email,
                    null,
                    appleId.ProductNavigation.Transactions
                        .Where(t => t.Direction == TransactionDirection.Sell)
                        .OrderByDescending(t => t.Date)
                        .Select(t => t.CustomerNavigation.PersonNavigation)
                        .Select(person => person.FirstName + " " + person.LastName)
                        .FirstOrDefault() ?? "Not sold",
                    appleId.ProductNavigation.GuaranteeProfile == null
                        ? "None"
                        : appleId.ProductNavigation.GuaranteeProfile.Corporation + " until " + appleId.ProductNavigation.GuaranteeProfile.ExpirationDate,
                    appleId.ProductNavigation.SecondHandProfile != null));
        }
        else
        {
            details = await phones.SelectAsync(
                id,
                phone => new ProductDetailsViewModel(
                    "Phone",
                    phone.ProductId,
                    phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name,
                    phone.ProductNavigation.ModelNavigation.Name,
                    string.IsNullOrWhiteSpace(phone.IMEI2)
                        ? "IMEI: " + phone.IMEI1
                        : "IMEI: " + phone.IMEI1 + " / " + phone.IMEI2,
                    phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name,
                    phone.ProductNavigation.Transactions
                        .Where(t => t.Direction == TransactionDirection.Sell)
                        .OrderByDescending(t => t.Date)
                        .Select(t => t.CustomerNavigation.PersonNavigation)
                        .Select(person => person.FirstName + " " + person.LastName)
                        .FirstOrDefault() ?? "Not sold",
                    phone.ProductNavigation.GuaranteeProfile == null
                        ? "None"
                        : phone.ProductNavigation.GuaranteeProfile.Corporation + " until " + phone.ProductNavigation.GuaranteeProfile.ExpirationDate.ToString("d"),
                    phone.ProductNavigation.SecondHandProfile != null));
        }

        if (details is null) return null;

        var rows = await GetProductTransactionsAsync(details.ProductId);
        return details with { Transactions = rows };
    }

    private async Task<IReadOnlyList<ProductTransactionViewModel>> GetProductTransactionsAsync(int productId)
        => (await transactions.SelectAllAsync(
                transaction => transaction.ProductId == productId,
                ProductTransactionProjection.Selector))
            .OrderByDescending(item => item.Date)
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropdownOptionViewModel>> GetManufacturersAsync()
        => (await manufacturers.FindAllAsync()).Select(m => new DropdownOptionViewModel(m.Id, m.Name)).ToList().AsReadOnly();

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropdownOptionViewModel>> GetModelsAsync(int manufacturerId)
        => (await models.FindAllAsync(m => m.ManufacturerId == manufacturerId)).Select(m => new DropdownOptionViewModel(m.Id, m.Name)).ToList().AsReadOnly();

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropdownOptionViewModel>> GetColorsAsync()
        => (await colors.FindAllAsync()).Select(c => new DropdownOptionViewModel(c.Id, c.Name)).ToList().AsReadOnly();

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetGuaranteeCorporationsAsync()
        => (await guarantees.FindAllAsync())
            .Select(g => g.Corporation)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.Ordinal)
            .ToList()
            .AsReadOnly();

    /// <inheritdoc />
    public async Task<DropdownCreateResult> CreateManufacturerAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new DropdownCreateResult(false, null, "Name is required.", 400);

        var trimmed = name.Trim();
        var existing = await manufacturers.FindAsync(m => m.Name == trimmed);
        if (existing is not null)
            return new DropdownCreateResult(true, new DropdownOptionViewModel(existing.Id, existing.Name), null, 200);

        var manufacturer = new Manufacturer { Name = trimmed };
        var added = await manufacturers.AddAsync(manufacturer) > 0;
        if (!added)
            return new DropdownCreateResult(false, null, "The manufacturer could not be saved.", 400);

        Logger.LogInformation("Added Manufacturer Id={Id}", manufacturer.Id);
        return new DropdownCreateResult(true, new DropdownOptionViewModel(manufacturer.Id, manufacturer.Name), null, 200);
    }

    /// <inheritdoc />
    public async Task<DropdownCreateResult> CreateModelAsync(int manufacturerId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new DropdownCreateResult(false, null, "Name is required.", 400);

        var manufacturer = await manufacturers.FindAsync(manufacturerId);
        if (manufacturer is null)
            return new DropdownCreateResult(false, null, "Manufacturer not found.", 404);

        var category = await categories.FindAsync(c => c.Name == "Phone")
            ?? throw new InvalidOperationException("The 'Phone' category is missing from the catalog seed data.");

        var trimmed = name.Trim();
        var existing = await models.FindAsync(m =>
            m.ManufacturerId == manufacturerId && m.Name == trimmed && m.CategoryId == category.Id);
        if (existing is not null)
            return new DropdownCreateResult(true, new DropdownOptionViewModel(existing.Id, existing.Name), null, 200);

        var model = new Model { ManufacturerId = manufacturerId, CategoryId = category.Id, Name = trimmed };
        var added = await models.AddAsync(model) > 0;
        if (!added)
            return new DropdownCreateResult(false, null, "The model could not be saved.", 400);

        Logger.LogInformation("Added Model Id={Id}", model.Id);
        return new DropdownCreateResult(true, new DropdownOptionViewModel(model.Id, model.Name), null, 200);
    }

    /// <inheritdoc />
    public async Task<DropdownCreateResult> CreateColorAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new DropdownCreateResult(false, null, "Name is required.", 400);

        var trimmed = name.Trim();
        var existing = await colors.FindAsync(c => c.Name == trimmed);
        if (existing is not null)
            return new DropdownCreateResult(true, new DropdownOptionViewModel(existing.Id, existing.Name), null, 200);

        var color = new MobileShop.Models.Entities.Color { Name = trimmed };
        var added = await colors.AddAsync(color) > 0;
        if (!added)
            return new DropdownCreateResult(false, null, "The color could not be saved.", 400);

        Logger.LogInformation("Added Color Id={Id}", color.Id);
        return new DropdownCreateResult(true, new DropdownOptionViewModel(color.Id, color.Name), null, 200);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CreatePhoneAsync(CreatePhoneInputModel input)
    {
        var imei1 = input.IMEI1.Trim();
        if (await phones.AnyAsync(p => p.IMEI1 == imei1))
            return new ServiceResult(false, "A phone with this IMEI already exists.", nameof(CreatePhoneInputModel.IMEI1), null);

        var manufacturer = await manufacturers.FindAsync(input.ManufacturerId);
        if (manufacturer is null)
            return new ServiceResult(false, "Selected manufacturer not found.", nameof(CreatePhoneInputModel.ManufacturerId), null);

        var model = await models.FindAsync(m =>
            m.Id == input.ModelId &&
            m.ManufacturerId == manufacturer.Id &&
            m.CategoryNavigation.Name == "Phone");
        if (model is null)
            return new ServiceResult(false, "Selected model not found for this manufacturer.", nameof(CreatePhoneInputModel.ModelId), null);

        MobileShop.Models.Entities.Color? color = null;
        if (input.ColorId.HasValue)
        {
            color = await colors.FindAsync(input.ColorId.Value);
            if (color is null)
                return new ServiceResult(false, "Selected color not found.", nameof(CreatePhoneInputModel.ColorId), null);
        }

        var product = new Product
        {
            ModelId = model.Id,
            ColorId = color?.Id,
            Barcode = Guid.NewGuid().ToString("N")[..12],
            Price = input.Price,
            SecondHandProfile = input.IsSecondHand ? new SecondHand
            {
                TestPeriodDays = input.TestPeriodDays ?? 30,
                UsedDurationDays = 0,
            } : null,
            GuaranteeProfile = input.HasGuarantee ? new Guarantee
            {
                StartDate = DateTime.Today,
                ExpirationDate = input.GuaranteeExpiry ?? DateTime.Today.AddYears(1),
                Corporation = string.IsNullOrWhiteSpace(input.GuaranteeCorporation) ? "Shop Warranty" : input.GuaranteeCorporation.Trim(),
            } : null,
        };

        var phone = new Phone
        {
            IMEI1 = imei1,
            IMEI2 = string.IsNullOrWhiteSpace(input.IMEI2) ? null : input.IMEI2.Trim(),
            OwnershipTransferred = false,
            ProductNavigation = product,
        };

        var result = await phones.AddAsync(phone) > 0;
        if (!result)
        {
            Logger.LogWarning("Failed to add Phone");
            return new ServiceResult(false, "The phone could not be saved. Check the details and try again.", null, null);
        }

        Logger.LogInformation("Added Phone Id={Id}", phone.Id);
        return new ServiceResult(true, null, null, phone.Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CreateAppleIdAsync(CreateAppleIdInputModel input)
    {
        var email = input.Email.Trim();
        if (await appleIds.AnyAsync(x => x.Email.ToLower() == email.ToLower()))
            return new ServiceResult(false, "This Apple ID email already exists.", nameof(CreateAppleIdInputModel.Email), null);

        var manufacturer = await manufacturers.FindAsync(m => m.Name == "Apple")
            ?? new Manufacturer { Name = "Apple" };

        if (manufacturer.Id == 0)
            await manufacturers.AddAsync(manufacturer);

        var category = await categories.FindAsync(c => c.Name == "AppleId")
            ?? throw new InvalidOperationException("The 'AppleId' category is missing from the catalog seed data.");

        var model = await models.FindAsync(m =>
                m.ManufacturerId == manufacturer.Id &&
                m.CategoryId == category.Id &&
                m.Name == "iPhone")
            ?? await AddModelAsync(manufacturer.Id, category.Id, "iPhone");

        var product = new Product
        {
            ModelId = model.Id,
            Barcode = Guid.NewGuid().ToString("N")[..12],
            Price = input.Price,
        };

        var appleId = new AppleId
        {
            Email = email,
            Password = input.Password.Trim(),
            Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
            ProductNavigation = product,
        };

        var result = await appleIds.AddAsync(appleId) > 0;
        if (!result)
        {
            Logger.LogWarning("Failed to add AppleId");
            return new ServiceResult(false, "The Apple ID could not be saved.", null, null);
        }

        Logger.LogInformation("Added AppleId Id={Id}", appleId.Id);
        return new ServiceResult(true, null, null, appleId.Id);
    }

    private async Task<Model> AddModelAsync(int manufacturerId, int categoryId, string name)
    {
        var model = new Model { ManufacturerId = manufacturerId, CategoryId = categoryId, Name = name };
        await models.AddAsync(model);
        return model;
    }
}
