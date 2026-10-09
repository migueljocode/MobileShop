namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when editing a product.</summary>
public sealed class EditProductInputModel
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Identifier { get; set; } = string.Empty;

    public string? Color { get; set; }
    public int? ColorId { get; set; }

    [Range(0, MoneyLimits.MaxRials)]
    public long Price { get; set; }

    [Range(0, 100)]
    public decimal? ProfitPercent { get; set; }

    [Range(0, MoneyLimits.MaxRials)]
    public long? ProfitAmount { get; set; }

    public bool IsSecondHand { get; set; }

    [StringLength(100)]
    public string? GuaranteeCorporation { get; set; }

    [DataType(DataType.Date)]
    public DateTime? GuaranteeExpiry { get; set; }

    [StringLength(500)]
    public string? GuaranteeNotes { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Second-hand specific
    [Range(0, int.MaxValue)]
    public int? TestPeriodDays { get; set; }

    [Range(0, int.MaxValue)]
    public int? UsedDurationDays { get; set; }

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? SecondHandNotes { get; set; }

    // Phone-specific
    [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "IMEI must be exactly 15 digits.")]
    public string? IMEI1 { get; set; }

    [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "IMEI must be exactly 15 digits.")]
    public string? IMEI2 { get; set; }

    public int? PartNumberId { get; set; }
    public int? ModelId { get; set; }
    public bool SupportsDualSim { get; set; }
    public bool SupportsEsim { get; set; }

    // Laptop-specific
    public string? Cpu { get; set; }
    public string? Gpu { get; set; }
    public decimal? DisplaySize { get; set; }

    // Apple ID-specific
    [StringLength(100)]
    public string? AppleIdPassword { get; set; }

    // Cable-specific
    public CableConnector? Connector1 { get; set; }
    public CableConnector? Connector2 { get; set; }
    public decimal? CableLength { get; set; }

    // Charger-specific
    public int? Wattage { get; set; }
    public bool? Pd { get; set; }
    public int? PortCount { get; set; }

    // Power Bank-specific
    public int? CapacityMah { get; set; }
    public int? MaxWattage { get; set; }
    public List<string> PortTypes { get; set; } = [];

    // Portable Storage-specific
    public StorageKind? StorageKind { get; set; }
    public int? StorageCapacityId { get; set; }
    public string? StorageCapacityLabel { get; set; }
    public int? Speed { get; set; }

    // Case/Glass-specific
    public List<string> CompatibleModels { get; set; } = [];
}