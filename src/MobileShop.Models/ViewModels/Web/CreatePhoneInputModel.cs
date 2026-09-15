namespace MobileShop.Models.ViewModels.Web;

/// <summary>Input submitted when creating a phone product.</summary>
public sealed class CreatePhoneInputModel
{
    [Required]
    [StringLength(100)]
    public string Manufacturer { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "IMEI must be exactly 15 digits.")]
    public string IMEI1 { get; set; } = string.Empty;

    [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "IMEI must be exactly 15 digits.")]
    public string? IMEI2 { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    public bool IsSecondHand { get; set; }

    [Range(0, int.MaxValue)]
    public int? TestPeriodDays { get; set; }

    public bool HasGuarantee { get; set; }

    [StringLength(100)]
    public string? GuaranteeCorporation { get; set; }

    [DataType(DataType.Date)]
    public DateTime? GuaranteeExpiry { get; set; }
}
