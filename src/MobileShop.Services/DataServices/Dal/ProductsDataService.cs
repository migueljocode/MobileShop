using MobileShop.Services.DataServices.Shared;

namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the catalog and creation operations for the Products area.</summary>
/// <remarks>
/// The nine repositories cover both product types plus the shared catalog entities and transactions. There is no
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
    IBaseRepo<PartNumber> partNumbers,
    IBaseRepo<Product> products,
    ILogger<ProductsDataService> logger)
    : IProductsDataService
{
    // Glass creation represents stock intake, so its paid cost is recorded as a Buy leg.
    // The seeded shop sentinel is used because the Create Glass form intentionally has no seller field.
    private const string PhoneCategoryName = "Phone";
    private const int ShopSellerId = 1;
    private const int ShopCustomerId = 1;
        /// <summary>Gets the structured logger for this products service.</summary>
    protected ILogger<ProductsDataService> Logger { get; } = logger;

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync(string? type = null, int? partNumberId = null, string? availability = null)
    {
        var rows = new List<ProductListItemViewModel>();

        // A positive part number narrows the list to the phones carrying it; zero/negative/null
        // is treated as no selection so the filter never hides everything by accident.
        var hasPartNumberFilter = partNumberId is > 0;

        // "phone" and "appleid" keep their dedicated views; "glass" is a dedicated glass view.
        // Null, "all" and unrecognised values return the complete inventory. Pages lowercase before calling.
        if (type is not "appleid" and not "glass")
        {
            Expression<Func<Phone, ProductListItemViewModel>> project = phone => new ProductListItemViewModel(
                phone.Id,
                phone.ProductId,
                "Phone",
                phone.ProductNavigation.ModelNavigation.ManufacturerNavigation.Name + " " + phone.ProductNavigation.ModelNavigation.Name,
                "IMEI: " + phone.IMEI1,
                phone.ProductNavigation.ColorNavigation == null ? null : phone.ProductNavigation.ColorNavigation.Name,
                phone.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                phone.ProductNavigation.SecondHandProfile != null)
            {
                PartNumberLabel = phone.PartNumberNavigation == null ? "N/A" : phone.PartNumberNavigation.Code,
            };

            // The predicate is only applied when a part number is actually selected, so a
            // zero/negative id can never silently drop every phone.
            var phoneRows = hasPartNumberFilter
                ? await phones.SelectAllAsync(phone => phone.PartNumberId == partNumberId, project)
                : await phones.SelectAllAsync(project);

            rows.AddRange(phoneRows.OrderBy(row => row.ProductId));
        }

        // Apple IDs never carry a part number, so an active part-number filter excludes them entirely.
        if (type is not "phone" and not "glass" && !hasPartNumberFilter)
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

        // Glass is stored as a Product with a Glass profile, so it has no separate subtype repository.
        if (type is not "phone" and not "appleid")
        {
            Expression<Func<Product, ProductListItemViewModel>> project = product => new ProductListItemViewModel(
                product.Id,
                product.Id,
                "Glass",
                product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name,
                "Barcode: " + product.Barcode,
                null,
                product.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                product.SecondHandProfile != null);

            rows.AddRange((await products.SelectAllAsync(product => product.GlassProfile != null, project))
                .OrderBy(row => row.ProductId));
        }

        var filteredRows = availability switch
        {
            "available" => rows.Where(row => !row.IsSold),
            "sold" => rows.Where(row => row.IsSold),
            _ => rows,
        };

        return filteredRows.ToList().AsReadOnly();
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
                phone.ProductNavigation.SecondHandProfile != null)
            {
                PartNumberLabel = phone.PartNumberNavigation == null ? "N/A" : phone.PartNumberNavigation.Code,
            }))
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

        if (string.Equals(type, "glass", StringComparison.OrdinalIgnoreCase))
        {
            details = await products.SelectAsync(
                id,
                product => new ProductDetailsViewModel(
                    "Glass",
                    product.Id,
                    product.ModelNavigation.ManufacturerNavigation.Name,
                    product.ModelNavigation.Name,
                    "Barcode: " + product.Barcode,
                    null,
                    product.Transactions
                        .Where(t => t.Direction == TransactionDirection.Sell)
                        .OrderByDescending(t => t.Date)
                        .Select(t => t.CustomerNavigation.PersonNavigation)
                        .Select(person => person.FirstName + " " + person.LastName)
                        .FirstOrDefault() ?? "Not sold",
                    product.GuaranteeProfile == null
                        ? "None"
                        : product.GuaranteeProfile.Corporation + " until " + product.GuaranteeProfile.ExpirationDate,
                    product.SecondHandProfile != null));
        }
        else if (string.Equals(type, "appleid", StringComparison.OrdinalIgnoreCase))
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
                    phone.ProductNavigation.SecondHandProfile != null)
                {
                    // A phone without a part number keeps the "N/A" defaults, so a missing part number
                    // is never rendered as a false capability.
                    PartNumberLabel = phone.PartNumberNavigation == null ? "N/A" : phone.PartNumberNavigation.Code,
                    DualSimLabel = phone.PartNumberNavigation == null ? "N/A" : phone.PartNumberNavigation.SupportsDualSim ? "Yes" : "No",
                    EsimLabel = phone.PartNumberNavigation == null ? "N/A" : phone.PartNumberNavigation.SupportsEsim ? "Yes" : "No",
                });
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
    public async Task<IReadOnlyList<DropdownOptionViewModel>> GetModelsAsync(int manufacturerId, string categoryName = PhoneCategoryName)
        => (await models.FindAllAsync(m => m.ManufacturerId == manufacturerId && m.CategoryNavigation.Name == categoryName))
            .Select(m => new DropdownOptionViewModel(m.Id, m.Name))
            .ToList()
            .AsReadOnly();

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropdownOptionViewModel>> GetColorsAsync()
        => (await colors.FindAllAsync()).Select(c => new DropdownOptionViewModel(c.Id, c.Name)).ToList().AsReadOnly();

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropdownOptionViewModel>> GetPartNumbersAsync(int? modelId = null)
        => (modelId is null
                ? await partNumbers.FindAllAsync()
                : await partNumbers.FindAllAsync(pn => pn.ModelId == modelId))
            .Select(pn => new DropdownOptionViewModel(pn.Id, pn.Code))
            .ToList()
            .AsReadOnly();

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropdownOptionViewModel>> GetInventoryPartNumbersAsync()
    {
        // Only part numbers actually attached to a live phone row are offered, so a catalog part
        // number with no inventory behind it never appears in the Products list filter.
        var usedIds = (await phones.SelectAllAsync(phone => phone.PartNumberId))
            .Where(id => id is > 0)
            .Select(id => id!.Value)
            .ToHashSet();

        if (usedIds.Count == 0)
            return [];

        return (await partNumbers.FindAllAsync(pn => usedIds.Contains(pn.Id)))
            .Select(pn => new DropdownOptionViewModel(pn.Id, pn.Code))
            .OrderBy(option => option.Name, StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();
    }

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
    public async Task<DropdownCreateResult> CreatePartNumberAsync(int modelId, string code, bool supportsDualSim, bool supportsEsim)
    {
        if (string.IsNullOrWhiteSpace(code))
            return new DropdownCreateResult(false, null, "Code is required.", 400);

        var model = await models.FindAsync(modelId);
        if (model is null)
            return new DropdownCreateResult(false, null, "Model not found.", 404);

        var trimmed = code.Trim();
        var existing = await partNumbers.FindAsync(pn => pn.ModelId == modelId && pn.Code == trimmed);
        if (existing is not null)
            return new DropdownCreateResult(true, new DropdownOptionViewModel(existing.Id, existing.Code), null, 200);

        var partNumber = new PartNumber
        {
            ModelId = modelId,
            Code = trimmed,
            SupportsDualSim = supportsDualSim,
            SupportsEsim = supportsEsim
        };
        var added = await partNumbers.AddAsync(partNumber) > 0;
        if (!added)
            return new DropdownCreateResult(false, null, "The part number could not be saved.", 400);

        Logger.LogInformation("Added PartNumber Id={Id} for ModelId={ModelId}", partNumber.Id, modelId);
        return new DropdownCreateResult(true, new DropdownOptionViewModel(partNumber.Id, partNumber.Code), null, 200);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CreatePhoneAsync(CreatePhoneInputModel input)
    {
        if (input.Price > MoneyLimits.MaxRials)
            return new ServiceResult(false, "The price is too large.", nameof(CreatePhoneInputModel.Price), null);
        if (input.ProfitAmount > MoneyLimits.MaxRials)
            return new ServiceResult(false, "The profit amount is too large.", nameof(CreatePhoneInputModel.ProfitAmount), null);

        if (!TryComputeFinishedPrice(input.Price, input.ProfitPercent, input.ProfitAmount, out var finishedPrice))
            return new ServiceResult(false, "The price is too large.", nameof(CreatePhoneInputModel.Price), null);

        var imei1 = input.IMEI1.Trim();
        if (await phones.AnyAsync(p => p.IMEI1 == imei1))
            return new ServiceResult(false, "A phone with this IMEI already exists.", nameof(CreatePhoneInputModel.IMEI1), null);

        var manufacturer = await manufacturers.FindAsync(input.ManufacturerId);
        if (manufacturer is null)
            return new ServiceResult(false, "Selected manufacturer not found.", nameof(CreatePhoneInputModel.ManufacturerId), null);

        var model = await models.FindAsync(m =>
            m.Id == input.ModelId &&
            m.ManufacturerId == manufacturer.Id &&
            m.CategoryNavigation.Name == PhoneCategoryName);
        if (model is null)
            return new ServiceResult(false, "Selected model not found for this manufacturer.", nameof(CreatePhoneInputModel.ModelId), null);

        MobileShop.Models.Entities.Color? color = null;
        if (input.ColorId.HasValue)
        {
            color = await colors.FindAsync(input.ColorId.Value);
            if (color is null)
                return new ServiceResult(false, "Selected color not found.", nameof(CreatePhoneInputModel.ColorId), null);
        }

        // The part number is optional, but when supplied it must belong to the selected model —
        // otherwise a phone could be created carrying another model's part number.
        PartNumber? partNumber = null;
        if (input.PartNumberId.HasValue)
        {
            partNumber = await partNumbers.FindAsync(input.PartNumberId.Value);
            if (partNumber is null)
                return new ServiceResult(false, "Selected part number not found.", nameof(CreatePhoneInputModel.PartNumberId), null);

            if (partNumber.ModelId != model.Id)
                return new ServiceResult(false, "The selected part number does not belong to the selected model.", nameof(CreatePhoneInputModel.PartNumberId), null);
        }

        var product = new Product
        {
            ModelId = model.Id,
            ColorId = color?.Id,
            Barcode = Guid.NewGuid().ToString("N")[..12],
            Price = finishedPrice,
            SecondHandProfile = input.IsSecondHand ? new SecondHand
            {
                TestPeriodDays = input.TestPeriodDays ?? 30,
                UsedDurationDays = 0,
                Notes = NormalizeNote(input.SecondHandNotes),
            } : null,
            GuaranteeProfile = input.HasGuarantee ? new Guarantee
            {
                StartDate = DateTime.Today,
                ExpirationDate = input.GuaranteeExpiry ?? DateTime.Today.AddYears(1),
                Corporation = string.IsNullOrWhiteSpace(input.GuaranteeCorporation) ? "Shop Warranty" : input.GuaranteeCorporation.Trim(),
                Notes = NormalizeNote(input.GuaranteeNotes),
            } : null,
        };

        var phone = new Phone
        {
            IMEI1 = imei1,
            IMEI2 = string.IsNullOrWhiteSpace(input.IMEI2) ? null : input.IMEI2.Trim(),
            OwnershipTransferred = false,
            PartNumberNavigation = partNumber,
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
    public async Task<ServiceResult> CreateGlassesAsync(CreateGlassInputModel input)
    {
        if (input.Price > MoneyLimits.MaxRials)
            return new ServiceResult(false, "The price is too large.", nameof(CreateGlassInputModel.Price), null);
        if (input.ProfitAmount > MoneyLimits.MaxRials)
            return new ServiceResult(false, "The profit amount is too large.", nameof(CreateGlassInputModel.ProfitAmount), null);

        if (input.Count < 1)
            return new ServiceResult(false, "Count must be at least 1.", nameof(CreateGlassInputModel.Count), null);

        if (!TryComputeFinishedPrice(input.Price, input.ProfitPercent, input.ProfitAmount, out var finishedPrice))
            return new ServiceResult(false, "The price is too large.", nameof(CreateGlassInputModel.Price), null);

        var compatibleManufacturer = await manufacturers.FindAsync(input.CompatibleManufacturerId);
        if (compatibleManufacturer is null)
            return new ServiceResult(false, "Selected compatible phone manufacturer not found.", nameof(CreateGlassInputModel.CompatibleManufacturerId), null);

        var glassManufacturer = await manufacturers.FindAsync(input.GlassManufacturerId);
        if (glassManufacturer is null)
            return new ServiceResult(false, "Selected glass manufacturer not found.", nameof(CreateGlassInputModel.GlassManufacturerId), null);

        var compatibleModel = await models.FindAsync(m =>
            m.Id == input.CompatibleModelId &&
            m.ManufacturerId == compatibleManufacturer.Id);
        if (compatibleModel is null)
            return new ServiceResult(false, "Selected compatible model not found for this manufacturer.", nameof(CreateGlassInputModel.CompatibleModelId), null);

        var compatibleCategory = await categories.FindAsync(compatibleModel.CategoryId);
        if (compatibleCategory is null || compatibleCategory.Name is not ("Phone" or "Tablet" or "SmartWatch"))
            return new ServiceResult(false, "Selected model cannot be used for a glass product.", nameof(CreateGlassInputModel.CompatibleModelId), null);

        var glassCategory = await categories.FindAsync(c => c.Name == "Glass");
        if (glassCategory is null)
            return new ServiceResult(false, "The 'Glass' category is missing from the catalog seed data.", nameof(CreateGlassInputModel.GlassManufacturerId), null);

        var glassModelName = compatibleModel.Name + " Glass";
        var glassModel = await models.FindAsync(m =>
            m.ManufacturerId == glassManufacturer.Id &&
            m.CategoryId == glassCategory.Id &&
            m.Name == glassModelName);

        if (glassModel is null)
        {
            glassModel = new Model
            {
                ManufacturerId = glassManufacturer.Id,
                CategoryId = glassCategory.Id,
                Name = glassModelName,
            };
            await models.AddAsync(glassModel, persist: false);
        }

        var batch = new List<Product>(input.Count);

        for (var i = 0; i < input.Count; i++)
        {
            var product = new Product
            {
                ModelId = glassModel.Id,
                ModelNavigation = glassModel,
                Barcode = Guid.NewGuid().ToString("N")[..12],
                Price = finishedPrice,
            };

            var glass = new Glass
            {
                ProductNavigation = product,
            };

            glass.ModelFits.Add(new GlassModelFit
            {
                GlassNavigation = glass,
                ModelId = compatibleModel.Id,
                ModelNavigation = compatibleModel,
            });

            product.GlassProfile = glass;
            batch.Add(product);
        }

        var purchaseTransactions = batch.Select(product => new Transaction
        {
            ProductNavigation = product,
            SellerId = ShopSellerId,
            CustomerId = ShopCustomerId,
            FinishedPrice = input.Price,
            Date = DateTime.Today,
            Direction = TransactionDirection.Buy,
        }).ToList();

        // Track the purchase legs without persisting separately; the product batch save commits the
        // Product + Glass + GlassModelFit + Buy graph together.
        await transactions.AddRangeAsync(purchaseTransactions, persist: false);

        var saved = await products.AddRangeAsync(batch) > 0;
        if (!saved)
        {
            Logger.LogWarning("Failed to add glass batch of {Count} products", input.Count);
            return new ServiceResult(false, "The glass products could not be saved. Check the details and try again.", null, null);
        }

        Logger.LogInformation("Added glass batch of {Count} products for GlassModelId={GlassModelId} and CompatibleModelId={CompatibleModelId}", input.Count, glassModel.Id, compatibleModel.Id);
        return new ServiceResult(true, null, null, batch[0].Id);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CreateAppleIdAsync(CreateAppleIdInputModel input)
    {
        if (input.Price > MoneyLimits.MaxRials)
            return new ServiceResult(false, "The price is too large.", nameof(CreateAppleIdInputModel.Price), null);
        if (input.ProfitAmount > MoneyLimits.MaxRials)
            return new ServiceResult(false, "The profit amount is too large.", nameof(CreateAppleIdInputModel.ProfitAmount), null);

        if (!TryComputeFinishedPrice(input.Price, input.ProfitPercent, input.ProfitAmount, out var finishedPrice))
            return new ServiceResult(false, "The price is too large.", nameof(CreateAppleIdInputModel.Price), null);

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
            Price = finishedPrice,
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

    /// <summary>
    /// Computes the finished price stored on <see cref="Product.Price"/> from the paid cost and the
    /// profit inputs, using the agreed amount-first rule: a supplied amount wins, otherwise a
    /// supplied percent is applied, otherwise the paid price is the finished price. Amount-first is
    /// a safety net for partial/stale posts — when the client keeps percent and amount in sync both
    /// branches agree. The result is never negative.
    /// </summary>
    private static bool TryComputeFinishedPrice(long paid, decimal? percent, long? amount, out long finished)
    {
        finished = 0;

        try
        {
            var value = amount.HasValue
                ? (decimal)paid + amount.Value
                : percent.HasValue
                    ? (decimal)paid + Math.Floor((decimal)paid * percent.Value / 100m)
                    : paid;

            if (value < 0)
                value = 0;

            if (value > MoneyLimits.MaxRials)
                return false;

            finished = (long)value;
            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    /// <summary>Trims a free-text note and maps whitespace-only input to null.</summary>
    private static string? NormalizeNote(string? note)
        => string.IsNullOrWhiteSpace(note) ? null : note.Trim();
}
