using System.Text.RegularExpressions;
using MobileShop.Services.DataServices.Shared;

namespace MobileShop.Services.DataServices.Dal;

/// <summary>Provides the catalog and creation operations for the Products area.</summary>
/// <remarks>
/// The repositories cover product types, shared catalog entities (including CPU/GPU lookups), and transactions. There is no
/// <see cref="Product"/> repo because each create path builds the product row inline and it is persisted by
/// cascade through the phone or Apple-ID insert, and no <see cref="SecondHand"/> repo (the second-hand profile is
/// a <see cref="Product"/> navigation, not a separate write).
/// </remarks>
public class ProductsDataService(
    IBaseRepo<Phone> phones,
    IBaseRepo<AppleId> appleIds,
    IBaseRepo<Seller> sellers,
    IBaseRepo<Customer> customers,
    IBaseRepo<Person> people,
    IBaseRepo<Manufacturer> manufacturers,
    IBaseRepo<Model> models,
    IBaseRepo<Category> categories,
    IBaseRepo<MobileShop.Models.Entities.Color> colors,
    IBaseRepo<Guarantee> guarantees,
    IBaseRepo<Transaction> transactions,
    IBaseRepo<PartNumber> partNumbers,
    IBaseRepo<Product> products,
    IBaseRepo<StorageCapacity> storageCapacities,
    IBaseRepo<Cpu> cpus,
    IBaseRepo<Gpu> gpus,
    ILogger<ProductsDataService> logger)
    : IProductsDataService
{
    // Product creation represents stock intake, so its paid cost is recorded as a Buy leg.
    private const string PhoneCategoryName = "Phone";
        /// <summary>Gets the structured logger for this products service.</summary>
    protected ILogger<ProductsDataService> Logger { get; } = logger;

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItemViewModel>> GetInventoryRowsAsync(string? type = null, int? partNumberId = null, string? availability = null)
    {
        var rows = new List<ProductListItemViewModel>();
        var normalizedType = type?.Trim().ToLowerInvariant();
        var showAll = string.IsNullOrEmpty(normalizedType) || normalizedType == "all" || normalizedType is not ("phone" or "appleid" or "glass" or "tablet" or "smartwatch" or "laptop" or "cable" or "charger" or "powerbank" or "portablestorage" or "case");
        var hasPartNumberFilter = partNumberId is > 0;
        if (showAll || normalizedType == "phone")
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
        if ((showAll || normalizedType == "appleid") && !hasPartNumberFilter)
            rows.AddRange((await appleIds.SelectAllAsync(
                appleId => new ProductListItemViewModel(
                    appleId.Id,
                    appleId.ProductId,
                    "Apple ID",
                    appleId.ProductNavigation.Transactions
                        .Where(t => t.Direction == TransactionDirection.Sell)
                        .OrderByDescending(t => t.Date)
                        .Select(t => t.CustomerNavigation.PersonNavigation == null ? null : t.CustomerNavigation.PersonNavigation.FirstName + " " + t.CustomerNavigation.PersonNavigation.LastName)
                        .FirstOrDefault() ?? "N/A",
                    appleId.Email,
                    null,
                    appleId.ProductNavigation.Transactions.Any(t => t.Direction == TransactionDirection.Sell),
                    appleId.ProductNavigation.SecondHandProfile != null)))
                .OrderBy(row => row.ProductId));

        // Glass is stored as a Product with a Glass profile, so it has no separate subtype repository.
        if (showAll || normalizedType == "glass")
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

        if (showAll || normalizedType == "tablet")
            rows.AddRange((await products.SelectAllAsync(product => product.TabletProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Tablet", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, product.ColorNavigation == null ? null : product.ColorNavigation.Name, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), product.SecondHandProfile != null))).OrderBy(row => row.ProductId));
        if (showAll || normalizedType == "smartwatch")
            rows.AddRange((await products.SelectAllAsync(product => product.SmartWatchProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Smart Watch", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, product.ColorNavigation == null ? null : product.ColorNavigation.Name, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), product.SecondHandProfile != null))).OrderBy(row => row.ProductId));
        if (showAll || normalizedType == "laptop")
            rows.AddRange((await products.SelectAllAsync(product => product.LaptopProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Laptop", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, product.ColorNavigation == null ? null : product.ColorNavigation.Name, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), product.SecondHandProfile != null))).OrderBy(row => row.ProductId));
        if (showAll || normalizedType == "cable")
            rows.AddRange((await products.SelectAllAsync(product => product.CableProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Cable", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, null, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), product.SecondHandProfile != null))).OrderBy(row => row.ProductId));
        if (showAll || normalizedType == "charger")
            rows.AddRange((await products.SelectAllAsync(product => product.ChargerProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Charger", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, null, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), product.SecondHandProfile != null))).OrderBy(row => row.ProductId));
        if (showAll || normalizedType == "powerbank")
            rows.AddRange((await products.SelectAllAsync(product => product.PowerBankProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Power Bank", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, null, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), product.SecondHandProfile != null))).OrderBy(row => row.ProductId));
        if (showAll || normalizedType == "portablestorage")
            rows.AddRange((await products.SelectAllAsync(product => product.PortableStorageProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Portable Storage", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, null, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), product.SecondHandProfile != null))).OrderBy(row => row.ProductId));
        if (showAll || normalizedType == "case")
            rows.AddRange((await products.SelectAllAsync(product => product.CaseProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Case", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, null, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), product.SecondHandProfile != null))).OrderBy(row => row.ProductId));

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

        var tabletRows = (await products.SelectAllAsync(product => product.TabletProfile != null && product.SecondHandProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Tablet", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, product.ColorNavigation == null ? null : product.ColorNavigation.Name, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), true))).OrderBy(row => row.ProductId).ToList();
        var watchRows = (await products.SelectAllAsync(product => product.SmartWatchProfile != null && product.SecondHandProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Smart Watch", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, product.ColorNavigation == null ? null : product.ColorNavigation.Name, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), true))).OrderBy(row => row.ProductId).ToList();
        var laptopRows = (await products.SelectAllAsync(product => product.LaptopProfile != null && product.SecondHandProfile != null, product => new ProductListItemViewModel(product.Id, product.Id, "Laptop", product.ModelNavigation.ManufacturerNavigation.Name + " " + product.ModelNavigation.Name, "Barcode: " + product.Barcode, product.ColorNavigation == null ? null : product.ColorNavigation.Name, product.Transactions.Any(t => t.Direction == TransactionDirection.Sell), true))).OrderBy(row => row.ProductId).ToList();
        return phoneRows.Concat(appleRows).Concat(tabletRows).Concat(watchRows).Concat(laptopRows).ToList().AsReadOnly();
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
                    appleId.ProductNavigation.SecondHandProfile != null)
                {
                    AppleIdPassword = appleId.Password,
                    Notes = appleId.Notes,
                });
        }
        else if (string.Equals(type, "tablet", StringComparison.OrdinalIgnoreCase))
        {
            details = await products.SelectAsync(id, product => new ProductDetailsViewModel("Tablet", product.Id, product.ModelNavigation.ManufacturerNavigation.Name, product.ModelNavigation.Name, "Barcode: " + product.Barcode, product.ColorNavigation == null ? null : product.ColorNavigation.Name, product.Transactions.Where(t => t.Direction == TransactionDirection.Sell).OrderByDescending(t => t.Date).Select(t => t.CustomerNavigation.PersonNavigation).Select(person => person.FirstName + " " + person.LastName).FirstOrDefault() ?? "Not sold", product.GuaranteeProfile == null ? "None" : product.GuaranteeProfile.Corporation + " until " + product.GuaranteeProfile.ExpirationDate.ToString("d"), product.SecondHandProfile != null) { Notes = product.TabletProfile!.Notes });
        }
        else if (string.Equals(type, "smartwatch", StringComparison.OrdinalIgnoreCase))
        {
            details = await products.SelectAsync(id, product => new ProductDetailsViewModel("Smart Watch", product.Id, product.ModelNavigation.ManufacturerNavigation.Name, product.ModelNavigation.Name, "Barcode: " + product.Barcode, product.ColorNavigation == null ? null : product.ColorNavigation.Name, product.Transactions.Where(t => t.Direction == TransactionDirection.Sell).OrderByDescending(t => t.Date).Select(t => t.CustomerNavigation.PersonNavigation).Select(person => person.FirstName + " " + person.LastName).FirstOrDefault() ?? "Not sold", product.GuaranteeProfile == null ? "None" : product.GuaranteeProfile.Corporation + " until " + product.GuaranteeProfile.ExpirationDate.ToString("d"), product.SecondHandProfile != null) { Notes = product.SmartWatchProfile!.Notes });
        }
        else if (string.Equals(type, "laptop", StringComparison.OrdinalIgnoreCase))
        {
            details = await products.SelectAsync(id, product => new ProductDetailsViewModel("Laptop", product.Id, product.ModelNavigation.ManufacturerNavigation.Name, product.ModelNavigation.Name, "Barcode: " + product.Barcode, product.ColorNavigation == null ? null : product.ColorNavigation.Name, product.Transactions.Where(t => t.Direction == TransactionDirection.Sell).OrderByDescending(t => t.Date).Select(t => t.CustomerNavigation.PersonNavigation).Select(person => person.FirstName + " " + person.LastName).FirstOrDefault() ?? "Not sold", product.GuaranteeProfile == null ? "None" : product.GuaranteeProfile.Corporation + " until " + product.GuaranteeProfile.ExpirationDate.ToString("d"), product.SecondHandProfile != null) { Cpu = product.LaptopProfile!.CpuNavigation.Name, Gpu = product.LaptopProfile.GpuNavigation == null ? null : product.LaptopProfile.GpuNavigation.Name, DisplaySize = product.LaptopProfile.DisplaySize, Notes = product.LaptopProfile.Notes });
        }
        else if (string.Equals(type, "cable", StringComparison.OrdinalIgnoreCase))
        {
            details = await products.SelectAsync(id, product => new ProductDetailsViewModel("Cable", product.Id, product.ModelNavigation.ManufacturerNavigation.Name, product.ModelNavigation.Name, "Barcode: " + product.Barcode, null, product.Transactions.Where(t => t.Direction == TransactionDirection.Sell).OrderByDescending(t => t.Date).Select(t => t.CustomerNavigation.PersonNavigation).Select(person => person.FirstName + " " + person.LastName).FirstOrDefault() ?? "Not sold", product.GuaranteeProfile == null ? "None" : product.GuaranteeProfile.Corporation + " until " + product.GuaranteeProfile.ExpirationDate.ToString("d"), product.SecondHandProfile != null) { Connector1 = product.CableProfile!.Connector1, Connector2 = product.CableProfile.Connector2, CableLength = product.CableProfile.Length, Notes = product.CableProfile.Notes });
        }
        else if (string.Equals(type, "charger", StringComparison.OrdinalIgnoreCase))
        {
            details = await products.SelectAsync(id, product => new ProductDetailsViewModel("Charger", product.Id, product.ModelNavigation.ManufacturerNavigation.Name, product.ModelNavigation.Name, "Barcode: " + product.Barcode, null, product.Transactions.Where(t => t.Direction == TransactionDirection.Sell).OrderByDescending(t => t.Date).Select(t => t.CustomerNavigation.PersonNavigation).Select(person => person.FirstName + " " + person.LastName).FirstOrDefault() ?? "Not sold", product.GuaranteeProfile == null ? "None" : product.GuaranteeProfile.Corporation + " until " + product.GuaranteeProfile.ExpirationDate.ToString("d"), product.SecondHandProfile != null) { Wattage = product.ChargerProfile!.Wattage, Pd = product.ChargerProfile.Pd, PortCount = product.ChargerProfile.PortCount, Notes = product.ChargerProfile.Notes });
        }
        else if (string.Equals(type, "powerbank", StringComparison.OrdinalIgnoreCase))
        {
            details = await products.SelectAsync(id, product => new ProductDetailsViewModel("Power Bank", product.Id, product.ModelNavigation.ManufacturerNavigation.Name, product.ModelNavigation.Name, "Barcode: " + product.Barcode, null, product.Transactions.Where(t => t.Direction == TransactionDirection.Sell).OrderByDescending(t => t.Date).Select(t => t.CustomerNavigation.PersonNavigation).Select(person => person.FirstName + " " + person.LastName).FirstOrDefault() ?? "Not sold", product.GuaranteeProfile == null ? "None" : product.GuaranteeProfile.Corporation + " until " + product.GuaranteeProfile.ExpirationDate.ToString("d"), product.SecondHandProfile != null) { CapacityMah = product.PowerBankProfile!.CapacityMah, MaxWattage = product.PowerBankProfile.MaxWattage, PortCount = product.PowerBankProfile.PortCount, PortTypes = product.PowerBankProfile.Ports.OrderBy(port => port.PortNumber).Select(port => port.Connector).ToList(), Pd = product.PowerBankProfile.Pd, Notes = product.PowerBankProfile.Notes });
        }
        else if (string.Equals(type, "portablestorage", StringComparison.OrdinalIgnoreCase))
        {
            details = await products.SelectAsync(id, product => new ProductDetailsViewModel("Portable Storage", product.Id, product.ModelNavigation.ManufacturerNavigation.Name, product.ModelNavigation.Name, "Barcode: " + product.Barcode, null, product.Transactions.Where(t => t.Direction == TransactionDirection.Sell).OrderByDescending(t => t.Date).Select(t => t.CustomerNavigation.PersonNavigation).Select(person => person.FirstName + " " + person.LastName).FirstOrDefault() ?? "Not sold", product.GuaranteeProfile == null ? "None" : product.GuaranteeProfile.Corporation + " until " + product.GuaranteeProfile.ExpirationDate.ToString("d"), product.SecondHandProfile != null) { StorageKind = product.PortableStorageProfile!.Kind, StorageCapacityLabel = product.PortableStorageProfile.StorageCapacityNavigation.Gb + " GB", Speed = product.PortableStorageProfile.Speed, Notes = product.PortableStorageProfile.Notes });
        }
        else if (string.Equals(type, "case", StringComparison.OrdinalIgnoreCase))
        {
            details = await products.SelectAsync(id, product => new ProductDetailsViewModel("Case", product.Id, product.ModelNavigation.ManufacturerNavigation.Name, product.ModelNavigation.Name, "Barcode: " + product.Barcode, null, product.Transactions.Where(t => t.Direction == TransactionDirection.Sell).OrderByDescending(t => t.Date).Select(t => t.CustomerNavigation.PersonNavigation).Select(person => person.FirstName + " " + person.LastName).FirstOrDefault() ?? "Not sold", product.GuaranteeProfile == null ? "None" : product.GuaranteeProfile.Corporation + " until " + product.GuaranteeProfile.ExpirationDate.ToString("d"), product.SecondHandProfile != null) { CompatibleModels = product.CaseProfile!.ModelFits.Select(fit => fit.ModelNavigation.Name).ToList(), Notes = product.CaseProfile.Notes });
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
                    // A phone stores the active SIM capability itself so each unit can differ even when
                    // the same part number is shared across multiple physical devices.
                    PartNumberLabel = phone.PartNumberNavigation == null ? "N/A" : phone.PartNumberNavigation.Code,
                    DualSimLabel = phone.SupportsDualSim == null
                        ? (phone.PartNumberNavigation == null ? "N/A" : (phone.PartNumberNavigation.SupportsDualSim ? "Yes" : "No"))
                        : (phone.SupportsDualSim.Value ? "Yes" : "No"),
                    EsimLabel = phone.SupportsEsim == null
                        ? (phone.PartNumberNavigation == null ? "N/A" : (phone.PartNumberNavigation.SupportsEsim ? "Yes" : "No"))
                        : (phone.SupportsEsim.Value ? "Yes" : "No"),
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
    public async Task<IReadOnlyList<DropdownOptionViewModel>> GetStorageCapacitiesAsync()
        => (await storageCapacities.FindAllAsync())
            .OrderBy(c => c.Gb)
            .Select(c => new DropdownOptionViewModel(c.Id, $"{c.Gb} GB"))
            .ToList()
            .AsReadOnly();

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropdownOptionViewModel>> GetCpusAsync()
        => (await cpus.FindAllAsync())
            .OrderBy(cpu => cpu.Name)
            .Select(cpu => new DropdownOptionViewModel(cpu.Id, cpu.Name))
            .ToList()
            .AsReadOnly();

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropdownOptionViewModel>> GetGpusAsync()
        => (await gpus.FindAllAsync())
            .OrderBy(gpu => gpu.Name)
            .Select(gpu => new DropdownOptionViewModel(gpu.Id, gpu.Name))
            .ToList()
            .AsReadOnly();

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
    public async Task<DropdownCreateResult> CreateModelAsync(int manufacturerId, string name, string categoryName = "Phone")
    {
        if (string.IsNullOrWhiteSpace(name))
            return new DropdownCreateResult(false, null, "Name is required.", 400);

        var manufacturer = await manufacturers.FindAsync(manufacturerId);
        if (manufacturer is null)
            return new DropdownCreateResult(false, null, "Manufacturer not found.", 404);

        var category = await categories.FindAsync(c => c.Name == categoryName);
        if (category is null)
        {
            category = new Category { Name = categoryName };
            if (await categories.AddAsync(category) <= 0)
                return new DropdownCreateResult(false, null, $"The '{categoryName}' category could not be saved.", 500);
        }

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
    public async Task<DropdownCreateResult> CreateStorageCapacityAsync(int gb)
    {
        if (gb <= 0)
            return new DropdownCreateResult(false, null, "Storage capacity must be greater than zero.", 400);

        var existing = await storageCapacities.FindAsync(c => c.Gb == gb);
        if (existing is not null)
            return new DropdownCreateResult(true, new DropdownOptionViewModel(existing.Id, $"{existing.Gb} GB"), null, 200);

        var capacity = new StorageCapacity { Gb = gb };
        var added = await storageCapacities.AddAsync(capacity) > 0;
        if (!added)
            return new DropdownCreateResult(false, null, "The storage capacity could not be saved.", 400);

        Logger.LogInformation("Added StorageCapacity Id={Id} for {Gb} GB", capacity.Id, capacity.Gb);
        return new DropdownCreateResult(true, new DropdownOptionViewModel(capacity.Id, $"{capacity.Gb} GB"), null, 200);
    }

    /// <inheritdoc />
    public Task<DropdownCreateResult> CreateCpuAsync(string name)
        => CreateNamedLookupAsync(name, cpus, "CPU", () => new Cpu(), (cpu, value) => cpu.Name = value, cpu => cpu.Name);

    /// <inheritdoc />
    public Task<DropdownCreateResult> CreateGpuAsync(string name)
        => CreateNamedLookupAsync(name, gpus, "GPU", () => new Gpu(), (gpu, value) => gpu.Name = value, gpu => gpu.Name);

    private async Task<DropdownCreateResult> CreateNamedLookupAsync<TEntity>(
        string name,
        IBaseRepo<TEntity> repository,
        string label,
        Func<TEntity> create,
        Action<TEntity, string> setName,
        Func<TEntity, string> getName)
        where TEntity : BaseEntity
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return new DropdownCreateResult(false, null, $"{label} name is required.", 400);
        if (trimmed.Length > 100)
            return new DropdownCreateResult(false, null, $"{label} name cannot exceed 100 characters.", 400);

        var existing = (await repository.FindAllAsync())
            .FirstOrDefault(entity => string.Equals(getName(entity), trimmed, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
            return new DropdownCreateResult(true, new DropdownOptionViewModel(existing.Id, getName(existing)), null, 200);

        var entity = create();
        setName(entity, trimmed);

        if (await repository.AddAsync(entity) <= 0)
            return new DropdownCreateResult(false, null, $"The {label} could not be saved.", 400);

        Logger.LogInformation("Added {LookupType} Id={Id}", label, entity.Id);
        return new DropdownCreateResult(true,
            new DropdownOptionViewModel(entity.Id, trimmed), null, 200);
    }

    /// <inheritdoc />
    public async Task<DropdownCreateResult> CreatePartNumberAsync(int modelId, string code, bool supportsDualSim = false, bool supportsEsim = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            return new DropdownCreateResult(false, null, "Code is required.", 400);

        var model = await models.FindAsync(modelId);
        if (model is null)
            return new DropdownCreateResult(false, null, "Model not found.", 404);

        var trimmed = code.Trim();
        if (!Regex.IsMatch(trimmed, @"^(?=.*[A-Za-z])(?=.*[\d/._-])[A-Za-z0-9/._-]{3,64}$"))
            return new DropdownCreateResult(false, null, "Use a realistic part number such as CH/ZAA, LL/A, MQ0K3LL/A, or SM-S921B.", 400);

        var existing = await partNumbers.FindAsync(pn => pn.ModelId == modelId && pn.Code == trimmed);
        if (existing is not null)
            return new DropdownCreateResult(true, new DropdownOptionViewModel(existing.Id, existing.Code), null, 200);

        var partNumber = new PartNumber
        {
            ModelId = modelId,
            Code = trimmed,
            SupportsDualSim = supportsDualSim,
            SupportsEsim = supportsEsim,
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

        var purchaseParties = await ResolvePurchasePartiesAsync(input.SellerId);
        if (purchaseParties.Failure is not null)
            return purchaseParties.Failure;

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
            SupportsDualSim = input.SupportsDualSim,
            SupportsEsim = input.SupportsEsim,
            OwnershipTransferred = false,
            PartNumberNavigation = partNumber,
            ProductNavigation = product,
        };

        await transactions.AddAsync(new Transaction
        {
            ProductNavigation = product,
            SellerId = purchaseParties.SellerId,
            CustomerId = purchaseParties.CustomerId,
            FinishedPrice = input.Price,
            Date = DateTime.Now,
            Direction = TransactionDirection.Buy,
        }, persist: false);

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

        var purchaseParties = await ResolvePurchasePartiesAsync(input.SellerId);
        if (purchaseParties.Failure is not null)
            return purchaseParties.Failure;

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
            SellerId = purchaseParties.SellerId,
            CustomerId = purchaseParties.CustomerId,
            FinishedPrice = input.Price,
            Date = DateTime.Now,
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

        var password = input.Password.Trim();
        if (string.IsNullOrWhiteSpace(password))
            return new ServiceResult(false, "Password is required.", nameof(CreateAppleIdInputModel.Password), null);

        var purchaseParties = await ResolvePurchasePartiesAsync(input.SellerId);
        if (purchaseParties.Failure is not null)
            return purchaseParties.Failure;

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
            Password = password,
            Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
            ProductNavigation = product,
        };

        await transactions.AddAsync(new Transaction
        {
            ProductNavigation = product,
            SellerId = purchaseParties.SellerId,
            CustomerId = purchaseParties.CustomerId,
            FinishedPrice = input.Price,
            Date = DateTime.Now,
            Direction = TransactionDirection.Buy,
        }, persist: false);

        var result = await appleIds.AddAsync(appleId) > 0;
        if (!result)
        {
            Logger.LogWarning("Failed to add AppleId");
            return new ServiceResult(false, "The Apple ID could not be saved.", null, null);
        }

        Logger.LogInformation("Added AppleId Id={Id}", appleId.Id);
        return new ServiceResult(true, null, null, appleId.Id);
    }


    public async Task<ServiceResult> CreateTabletAsync(CreateTabletInputModel input) => await CreateDeviceAsync(input.SellerId, input.ManufacturerId, input.ModelId, "Tablet", input.Price, input.ProfitPercent, input.ProfitAmount, input.IsSecondHand, input.TestPeriodDays, input.SecondHandNotes, input.HasGuarantee, input.GuaranteeCorporation, input.GuaranteeExpiry, input.GuaranteeNotes, new Tablet { Notes = NormalizeNote(input.Notes) });
    public async Task<ServiceResult> CreateSmartWatchAsync(CreateSmartWatchInputModel input) => await CreateDeviceAsync(input.SellerId, input.ManufacturerId, input.ModelId, "SmartWatch", input.Price, input.ProfitPercent, input.ProfitAmount, input.IsSecondHand, input.TestPeriodDays, input.SecondHandNotes, input.HasGuarantee, input.GuaranteeCorporation, input.GuaranteeExpiry, input.GuaranteeNotes, new SmartWatch { Notes = NormalizeNote(input.Notes) });
    public async Task<ServiceResult> CreateLaptopAsync(CreateLaptopInputModel input)
    {
        if (input.DisplaySize <= 0)
            return new ServiceResult(false, "Display size must be greater than 0.", null, null);

        return await CreateDeviceAsync(input.SellerId, input.ManufacturerId, input.ModelId, "Laptop", input.Price, input.ProfitPercent, input.ProfitAmount, input.IsSecondHand, input.TestPeriodDays, input.SecondHandNotes, input.HasGuarantee, input.GuaranteeCorporation, input.GuaranteeExpiry, input.GuaranteeNotes, new Laptop { CpuId = input.CpuId, GpuId = input.GpuId, DisplaySize = input.DisplaySize, Notes = NormalizeNote(input.Notes) });
    }

    public Task<ServiceResult> CreateCablesAsync(CreateCableInputModel input) =>
        CreateAccessoryBatchAsync(input.SellerId, input.Count, input.Price, input.ProfitPercent, input.ProfitAmount, input.ManufacturerId, input.ModelId, "Cable",
            product => product.CableProfile = new Cable { Connector1 = input.Connector1, Connector2 = input.Connector2, Length = input.Length });

    public Task<ServiceResult> CreateChargersAsync(CreateChargerInputModel input) =>
        CreateAccessoryBatchAsync(input.SellerId, input.Count, input.Price, input.ProfitPercent, input.ProfitAmount, input.ManufacturerId, input.ModelId, "Charger",
            product => product.ChargerProfile = new Charger { Wattage = input.Wattage, Pd = input.Pd, PortCount = input.PortCount });

    public Task<ServiceResult> CreatePowerBanksAsync(CreatePowerBankInputModel input)
    {
        if (input.PortCount < 1)
            return Task.FromResult(new ServiceResult(false, "Port count must be at least 1.", nameof(CreatePowerBankInputModel.PortCount), null));
        if (input.PortTypes is null || input.PortTypes.Count != input.PortCount)
            return Task.FromResult(new ServiceResult(false, "Select a connector type for every port.", nameof(CreatePowerBankInputModel.PortTypes), null));
        if (input.PortTypes.Any(type => !Enum.IsDefined(type)))
            return Task.FromResult(new ServiceResult(false, "Select a valid connector type for every port.", nameof(CreatePowerBankInputModel.PortTypes), null));

        return CreateAccessoryBatchAsync(input.SellerId, input.Count, input.Price, input.ProfitPercent, input.ProfitAmount, input.ManufacturerId, input.ModelId, "PowerBank",
            product => product.PowerBankProfile = new PowerBank
            {
                CapacityMah = input.CapacityMah,
                MaxWattage = input.MaxWattage,
                PortCount = input.PortCount,
                Pd = input.Pd,
                Ports = input.PortTypes.Select((connector, index) => new PowerBankPort
                {
                    PortNumber = index + 1,
                    Connector = connector
                }).ToList()
            });
    }

    public async Task<ServiceResult> CreatePortableStoragesAsync(CreatePortableStorageInputModel input)
    {
        if (!Enum.IsDefined(input.StorageKind))
            return new ServiceResult(false, "Select a valid storage kind.", nameof(CreatePortableStorageInputModel.StorageKind), null);
        if (await storageCapacities.FindAsync(capacity => capacity.Id == input.StorageCapacityId) is null)
            return new ServiceResult(false, "Select a valid storage capacity.", nameof(CreatePortableStorageInputModel.StorageCapacityId), null);

        return await CreateAccessoryBatchAsync(input.SellerId, input.Count, input.Price, input.ProfitPercent, input.ProfitAmount, input.ManufacturerId, input.ModelId, "PortableStorage",
            product => product.PortableStorageProfile = new PortableStorage { Kind = input.StorageKind, StorageCapacityId = input.StorageCapacityId, Speed = input.Speed });
    }

    public async Task<ServiceResult> CreateCasesAsync(CreateCaseInputModel input)
    {
        if (input.Count < 1)
            return new ServiceResult(false, "Count must be at least 1.", nameof(CreateCaseInputModel.Count), null);
        if (input.Price > MoneyLimits.MaxRials || input.ProfitAmount > MoneyLimits.MaxRials || !TryComputeFinishedPrice(input.Price, input.ProfitPercent, input.ProfitAmount, out var finishedPrice))
            return new ServiceResult(false, "The price is too large.", nameof(CreateCaseInputModel.Price), null);
        if (await manufacturers.FindAsync(input.ManufacturerId) is null)
            return new ServiceResult(false, "Selected manufacturer not found.", nameof(CreateCaseInputModel.ManufacturerId), null);
        var compatibleManufacturer = await manufacturers.FindAsync(input.CompatibleManufacturerId);
        if (compatibleManufacturer is null)
            return new ServiceResult(false, "Selected compatible manufacturer not found.", nameof(CreateCaseInputModel.CompatibleManufacturerId), null);
        var compatibleModels = new List<Model>();
        foreach (var modelId in input.CompatibleModelIds.Distinct())
        {
            var model = await models.FindAsync(m => m.Id == modelId && m.ManufacturerId == compatibleManufacturer.Id && m.CategoryNavigation.Name == "Phone");
            if (model is null)
                return new ServiceResult(false, "One or more selected compatible models were not found.", nameof(CreateCaseInputModel.CompatibleModelIds), null);
            compatibleModels.Add(model);
        }
        if (compatibleModels.Count == 0)
            return new ServiceResult(false, "Select at least one compatible model.", nameof(CreateCaseInputModel.CompatibleModelIds), null);

        var purchaseParties = await ResolvePurchasePartiesAsync(input.SellerId);
        if (purchaseParties.Failure is not null)
            return purchaseParties.Failure;

        var caseCategory = await categories.FindAsync(c => c.Name == "Case");
        if (caseCategory is null)
            return new ServiceResult(false, "The 'Case' category is missing from the catalog seed data.", nameof(CreateCaseInputModel.ManufacturerId), null);
        var caseModelName = compatibleModels[0].Name + " Case";
        var caseModel = await models.FindAsync(m => m.ManufacturerId == input.ManufacturerId && m.CategoryId == caseCategory.Id && m.Name == caseModelName);
        if (caseModel is null)
        {
            caseModel = new Model { ManufacturerId = input.ManufacturerId, CategoryId = caseCategory.Id, Name = caseModelName };
            await models.AddAsync(caseModel, persist: false);
        }

        var batch = new List<Product>(input.Count);
        var purchaseTransactions = new List<Transaction>(input.Count);
        for (var i = 0; i < input.Count; i++)
        {
            var product = new Product { ModelId = caseModel.Id, ModelNavigation = caseModel, Barcode = Guid.NewGuid().ToString("N")[..12], Price = finishedPrice };
            var profile = new Case { ProductNavigation = product, Notes = NormalizeNote(input.Notes) };
            foreach (var compatibleModel in compatibleModels)
                profile.ModelFits.Add(new CaseModelFit { CaseNavigation = profile, ModelId = compatibleModel.Id, ModelNavigation = compatibleModel });
            product.CaseProfile = profile;
            batch.Add(product);
            purchaseTransactions.Add(new Transaction { ProductNavigation = product, SellerId = purchaseParties.SellerId, CustomerId = purchaseParties.CustomerId, FinishedPrice = input.Price, Date = DateTime.Now, Direction = TransactionDirection.Buy });
        }
        await transactions.AddRangeAsync(purchaseTransactions, persist: false);
        if (await products.AddRangeAsync(batch) <= 0)
            return new ServiceResult(false, "The case products could not be saved. Check the details and try again.", null, null);
        return new ServiceResult(true, null, null, batch[0].Id);
    }

    public async Task<ServiceResult> UpdateProductAsync(EditProductInputModel input)
    {
        // The same includes as GetProductForEditAsync, tracked, so every profile the
        // type switch below needs is loaded and the changed entity can be saved
        // directly instead of re-attaching a half-loaded graph.
        var product = await products.FindTrackedWithIncludesAsync(
            input.ProductId,
            p => p.ModelNavigation,
            p => p.ModelNavigation.ManufacturerNavigation,
            p => p.ColorNavigation,
            p => p.SecondHandProfile,
            p => p.GuaranteeProfile,
            p => p.PhoneProfile,
            p => p.TabletProfile,
            p => p.SmartWatchProfile,
            p => p.LaptopProfile,
            p => p.AppleIdProfile,
            p => p.CableProfile,
            p => p.ChargerProfile,
            p => p.PowerBankProfile,
            p => p.PowerBankProfile.Ports,
            p => p.PortableStorageProfile,
            p => p.PortableStorageProfile.StorageCapacityNavigation,
            p => p.CaseProfile,
            p => p.CaseProfile.ModelFits,
            p => p.GlassProfile,
            p => p.GlassProfile.ModelFits);
        if (product is null)
            return new ServiceResult(false, "Product not found.", nameof(input.ProductId), null);

        if (!TryComputeFinishedPrice(input.Price, input.ProfitPercent, input.ProfitAmount, out var finishedPrice))
            return new ServiceResult(false, "The price is too large.", nameof(input.Price), null);

        // Update common fields
        product.ModelId = input.ModelId ?? product.ModelId;
        product.Price = finishedPrice;
        product.Barcode = input.Identifier;
        product.ColorId = input.ColorId > 0 ? input.ColorId : product.ColorId;

        // Update SecondHand profile
        if (input.IsSecondHand)
        {
            product.SecondHandProfile ??= new SecondHand();
            product.SecondHandProfile.TestPeriodDays = input.TestPeriodDays ?? 30;
            product.SecondHandProfile.UsedDurationDays = input.UsedDurationDays ?? 0;
            product.SecondHandProfile.Notes = NormalizeNote(input.SecondHandNotes);
        }
        else
        {
            product.SecondHandProfile = null;
        }

        // Update Guarantee profile
        if (!string.IsNullOrWhiteSpace(input.GuaranteeCorporation) || input.GuaranteeExpiry.HasValue)
        {
            product.GuaranteeProfile ??= new Guarantee();
            product.GuaranteeProfile.Corporation = string.IsNullOrWhiteSpace(input.GuaranteeCorporation) ? "Shop Warranty" : input.GuaranteeCorporation.Trim();
            product.GuaranteeProfile.ExpirationDate = input.GuaranteeExpiry ?? DateTime.Today.AddYears(1);
            product.GuaranteeProfile.Notes = NormalizeNote(input.GuaranteeNotes);
        }
        else
        {
            product.GuaranteeProfile = null;
        }

        // Update type-specific fields
        switch (input.Type)
        {
            case "Phone":
                if (product.PhoneProfile is null) return new ServiceResult(false, "Product is not a phone.", nameof(input.Type), null);
                product.PhoneProfile.IMEI1 = string.IsNullOrWhiteSpace(input.IMEI1) ? product.PhoneProfile.IMEI1 : input.IMEI1.Trim();
                product.PhoneProfile.IMEI2 = string.IsNullOrWhiteSpace(input.IMEI2) ? product.PhoneProfile.IMEI2 : input.IMEI2?.Trim();
                product.PhoneProfile.PartNumberId = input.PartNumberId ?? product.PhoneProfile.PartNumberId;
                product.PhoneProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Tablet":
                if (product.TabletProfile is null) return new ServiceResult(false, "Product is not a tablet.", nameof(input.Type), null);
                product.TabletProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Smart Watch":
                if (product.SmartWatchProfile is null) return new ServiceResult(false, "Product is not a smart watch.", nameof(input.Type), null);
                product.SmartWatchProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Laptop":
                if (product.LaptopProfile is null) return new ServiceResult(false, "Product is not a laptop.", nameof(input.Type), null);
                // CpuId and GpuId need to be resolved from names
                if (!string.IsNullOrWhiteSpace(input.Cpu))
                {
                    var cpu = await cpus.FindAsync(c => c.Name == input.Cpu);
                    if (cpu is not null) product.LaptopProfile.CpuId = cpu.Id;
                }
                if (!string.IsNullOrWhiteSpace(input.Gpu))
                {
                    var gpu = await gpus.FindAsync(g => g.Name == input.Gpu);
                    if (gpu is not null) product.LaptopProfile.GpuId = gpu.Id;
                }
                product.LaptopProfile.DisplaySize = input.DisplaySize ?? product.LaptopProfile.DisplaySize;
                product.LaptopProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Apple ID":
                if (product.AppleIdProfile is null) return new ServiceResult(false, "Product is not an Apple ID.", nameof(input.Type), null);
                product.AppleIdProfile.Password = string.IsNullOrWhiteSpace(input.AppleIdPassword) ? product.AppleIdProfile.Password : input.AppleIdPassword.Trim();
                product.AppleIdProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Cable":
                if (product.CableProfile is null) return new ServiceResult(false, "Product is not a cable.", nameof(input.Type), null);
                product.CableProfile.Connector1 = input.Connector1 ?? product.CableProfile.Connector1;
                product.CableProfile.Connector2 = input.Connector2 ?? product.CableProfile.Connector2;
                product.CableProfile.Length = input.CableLength ?? product.CableProfile.Length;
                product.CableProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Charger":
                if (product.ChargerProfile is null) return new ServiceResult(false, "Product is not a charger.", nameof(input.Type), null);
                product.ChargerProfile.Wattage = input.Wattage ?? product.ChargerProfile.Wattage;
                product.ChargerProfile.Pd = input.Pd ?? product.ChargerProfile.Pd;
                product.ChargerProfile.PortCount = input.PortCount ?? product.ChargerProfile.PortCount;
                product.ChargerProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Power Bank":
                if (product.PowerBankProfile is null) return new ServiceResult(false, "Product is not a power bank.", nameof(input.Type), null);
                product.PowerBankProfile.CapacityMah = input.CapacityMah ?? product.PowerBankProfile.CapacityMah;
                product.PowerBankProfile.MaxWattage = input.MaxWattage ?? product.PowerBankProfile.MaxWattage;
                product.PowerBankProfile.PortCount = input.PortCount ?? product.PowerBankProfile.PortCount;
                product.PowerBankProfile.Pd = input.Pd ?? product.PowerBankProfile.Pd;
                product.PowerBankProfile.Ports = input.PortTypes?.Select((connector, index) => new PowerBankPort
                {
                    PortNumber = index + 1,
                    Connector = Enum.Parse<CableConnector>(connector)
                }).ToList() ?? product.PowerBankProfile.Ports;
                product.PowerBankProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Portable Storage":
                if (product.PortableStorageProfile is null) return new ServiceResult(false, "Product is not a portable storage.", nameof(input.Type), null);
                product.PortableStorageProfile.Kind = input.StorageKind ?? product.PortableStorageProfile.Kind;
                product.PortableStorageProfile.StorageCapacityId = input.StorageCapacityId ?? product.PortableStorageProfile.StorageCapacityId;
                product.PortableStorageProfile.Speed = input.Speed ?? product.PortableStorageProfile.Speed;
                product.PortableStorageProfile.Notes = NormalizeNote(input.Notes);
                break;

            case "Case":
                if (product.CaseProfile is null) return new ServiceResult(false, "Product is not a case.", nameof(input.Type), null);
                product.CaseProfile.Notes = NormalizeNote(input.Notes);
                // Update compatible models
                if (input.CompatibleModels is not null)
                {
                    product.CaseProfile.ModelFits.Clear();
                    foreach (var modelName in input.CompatibleModels.Distinct())
                    {
                        var model = await models.FindAsync(m => m.Name == modelName);
                        if (model is not null)
                        {
                            product.CaseProfile.ModelFits.Add(new CaseModelFit { CaseNavigation = product.CaseProfile, ModelId = model.Id, ModelNavigation = model });
                        }
                    }
                }
                break;

            case "Glass":
                if (product.GlassProfile is null) return new ServiceResult(false, "Product is not a glass.", nameof(input.Type), null);
                product.GlassProfile.Notes = NormalizeNote(input.Notes);
                // Update compatible models
                if (input.CompatibleModels is not null)
                {
                    product.GlassProfile.ModelFits.Clear();
                    foreach (var modelName in input.CompatibleModels.Distinct())
                    {
                        var model = await models.FindAsync(m => m.Name == modelName);
                        if (model is not null)
                        {
                            product.GlassProfile.ModelFits.Add(new GlassModelFit { GlassNavigation = product.GlassProfile, ModelId = model.Id, ModelNavigation = model });
                        }
                    }
                }
                break;

            default:
                return new ServiceResult(false, $"Unsupported product type: {input.Type}", nameof(input.Type), null);
        }

        var updated = await products.SaveChangesAsync() > 0;
        if (!updated)
            return new ServiceResult(false, "The product could not be updated. Check the details and try again.", null, null);

        return new ServiceResult(true, "Product updated successfully.", null, input.ProductId);
    }

public async Task<Product?> GetProductForEditAsync(int id)
    {
        return await products.FindWithIncludesAsync(
            id,
            p => p.ModelNavigation,
            p => p.ModelNavigation.ManufacturerNavigation,
            p => p.ColorNavigation,
            p => p.SecondHandProfile,
            p => p.GuaranteeProfile,
            p => p.PhoneProfile,
            p => p.TabletProfile,
            p => p.SmartWatchProfile,
            p => p.LaptopProfile,
            p => p.AppleIdProfile,
            p => p.CableProfile,
            p => p.ChargerProfile,
            p => p.PowerBankProfile,
            p => p.PowerBankProfile.Ports,
            p => p.PortableStorageProfile,
            p => p.PortableStorageProfile.StorageCapacityNavigation,
            p => p.CaseProfile,
            p => p.CaseProfile.ModelFits,
            p => p.GlassProfile,
            p => p.GlassProfile.ModelFits);
    }

    private async Task<ServiceResult> CreateAccessoryBatchAsync(int sellerId, int count, long price, decimal? profitPercent, long? profitAmount, int manufacturerId, int modelId, string categoryName, Action<Product> configure)
    {
        if (count < 1)
            return new ServiceResult(false, "Count must be at least 1.", null, null);
        if (price > MoneyLimits.MaxRials || profitAmount > MoneyLimits.MaxRials || !TryComputeFinishedPrice(price, profitPercent, profitAmount, out var finishedPrice))
            return new ServiceResult(false, "The price is too large.", null, null);
        if (await manufacturers.FindAsync(manufacturerId) is null)
            return new ServiceResult(false, "Selected manufacturer not found.", nameof(manufacturerId), null);
        var model = await models.FindAsync(m => m.Id == modelId && m.ManufacturerId == manufacturerId && m.CategoryNavigation.Name == categoryName);
        if (model is null)
            return new ServiceResult(false, "Selected model not found for this manufacturer.", nameof(modelId), null);

        var purchaseParties = await ResolvePurchasePartiesAsync(sellerId);
        if (purchaseParties.Failure is not null)
            return purchaseParties.Failure;

        var batch = new List<Product>(count);
        var purchaseTransactions = new List<Transaction>(count);
        for (var i = 0; i < count; i++)
        {
            var product = new Product { ModelId = model.Id, ModelNavigation = model, Barcode = Guid.NewGuid().ToString("N")[..12], Price = finishedPrice };
            configure(product);
            batch.Add(product);
            purchaseTransactions.Add(new Transaction { ProductNavigation = product, SellerId = purchaseParties.SellerId, CustomerId = purchaseParties.CustomerId, FinishedPrice = price, Date = DateTime.Now, Direction = TransactionDirection.Buy });
        }
        await transactions.AddRangeAsync(purchaseTransactions, persist: false);
        if (await products.AddRangeAsync(batch) <= 0)
            return new ServiceResult(false, "The accessory products could not be saved. Check the details and try again.", null, null);
        return new ServiceResult(true, null, null, batch[0].Id);
    }
    private async Task<ServiceResult> CreateDeviceAsync(int sellerId, int manufacturerId, int modelId, string categoryName, long price, decimal? profitPercent, long? profitAmount, bool isSecondHand, int? testPeriodDays, string? secondHandNotes, bool hasGuarantee, string? guaranteeCorporation, DateTime? guaranteeExpiry, string? guaranteeNotes, object profile)
    {
        if (price > MoneyLimits.MaxRials || profitAmount > MoneyLimits.MaxRials || !TryComputeFinishedPrice(price, profitPercent, profitAmount, out var finishedPrice)) return new ServiceResult(false, "The price is too large.", null, null);
        if (await manufacturers.FindAsync(manufacturerId) is null) return new ServiceResult(false, "Selected manufacturer not found.", nameof(manufacturerId), null);
        var model = await models.FindAsync(m => m.Id == modelId && m.ManufacturerId == manufacturerId && m.CategoryNavigation.Name == categoryName);
        if (model is null) return new ServiceResult(false, "Selected model not found for this manufacturer.", nameof(modelId), null);
        var purchaseParties = await ResolvePurchasePartiesAsync(sellerId);
        if (purchaseParties.Failure is not null) return purchaseParties.Failure;
        var product = new Product { ModelId = model.Id, Barcode = Guid.NewGuid().ToString("N")[..12], Price = finishedPrice, SecondHandProfile = isSecondHand ? new SecondHand { TestPeriodDays = testPeriodDays ?? 30, UsedDurationDays = 0, Notes = NormalizeNote(secondHandNotes) } : null, GuaranteeProfile = hasGuarantee ? new Guarantee { StartDate = DateTime.Today, ExpirationDate = guaranteeExpiry ?? DateTime.Today.AddYears(1), Corporation = string.IsNullOrWhiteSpace(guaranteeCorporation) ? "Shop Warranty" : guaranteeCorporation.Trim(), Notes = NormalizeNote(guaranteeNotes) } : null };
        switch (profile) { case Tablet x: product.TabletProfile = x; break; case SmartWatch x: product.SmartWatchProfile = x; break; case Laptop x: product.LaptopProfile = x; break; default: throw new ArgumentException("Unsupported device profile.", nameof(profile)); }
        await transactions.AddAsync(new Transaction
        {
            ProductNavigation = product,
            SellerId = purchaseParties.SellerId,
            CustomerId = purchaseParties.CustomerId,
            FinishedPrice = price,
            Date = DateTime.Now,
            Direction = TransactionDirection.Buy,
        }, persist: false);
        var saved = await products.AddAsync(product) > 0;
        if (!saved) return new ServiceResult(false, "The product could not be saved. Check the details and try again.", null, null);
        var productId = product.Id;
        return new ServiceResult(true, null, null, productId);
    }

    private async Task<PurchasePartyResolution> ResolvePurchasePartiesAsync(int sellerId)
    {
        if (await sellers.FindAsync(sellerId) is null)
            return new PurchasePartyResolution(0, 0,
                new ServiceResult(false, "Selected seller not found.", "SellerId", null));

        var anis = await customers.FindAsync(customer =>
            customer.PersonNavigation.FirstName == "Anis" &&
            customer.PersonNavigation.LastName == "Sahabi");
        if (anis is null)
        {
            var anisPerson = await people.FindAsync(person =>
                person.FirstName == "Anis" && person.LastName == "Sahabi");
            if (anisPerson is null)
                return new PurchasePartyResolution(0, 0,
                    new ServiceResult(false, "Anis Sahabi must be registered as a person before creating products.", null, null));

            anis = new Customer { PersonId = anisPerson.Id, NationalId = "0099999999" };
            if (await customers.AddAsync(anis) <= 0)
                return new PurchasePartyResolution(0, 0,
                    new ServiceResult(false, "Anis Sahabi could not be registered as a customer.", null, null));
        }

        return new PurchasePartyResolution(sellerId, anis.Id, null);
    }

    private sealed record PurchasePartyResolution(int SellerId, int CustomerId, ServiceResult? Failure);

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
