namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when creating a phone product.</summary>
public sealed class CreatePhoneInputModel
{
    [Required]
    public int ManufacturerId { get; set; }

    [Required]
    public int ModelId { get; set; }

    [Range(0, double.MaxValue)]
    public int Price { get; set; }

    [Range(0, 100)]
    public decimal? ProfitPercent { get; set; }

    [Range(0, double.MaxValue)]
    public int? ProfitAmount { get; set; }

    [Required]
    [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "IMEI must be exactly 15 digits.")]
    public string IMEI1 { get; set; } = string.Empty;

    [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "IMEI must be exactly 15 digits.")]
    public string? IMEI2 { get; set; }

    public int? ColorId { get; set; }

    /// <summary>
    /// The optional part number for this phone. Omitted stays null, which keeps existing phones
    /// and callers valid; a value must belong to the selected model.
    /// </summary>
    public int? PartNumberId { get; set; }

    public bool IsSecondHand { get; set; }

    [Range(0, int.MaxValue)]
    public int? TestPeriodDays { get; set; }

    /// <summary>Optional free-text notes for a second-hand phone. Empty trims to null.</summary>
    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? SecondHandNotes { get; set; }

    public bool HasGuarantee { get; set; }

    [StringLength(100)]
    public string? GuaranteeCorporation { get; set; }

    [DataType(DataType.Date)]
    public DateTime? GuaranteeExpiry { get; set; }

    /// <summary>Optional free-text notes for the guarantee. Empty trims to null.</summary>
    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? GuaranteeNotes { get; set; }
}
