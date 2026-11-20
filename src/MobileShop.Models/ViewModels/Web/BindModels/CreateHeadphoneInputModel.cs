namespace MobileShop.Models.ViewModels.Web.BindModels;

public sealed class CreateHeadphoneInputModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a seller.")]
    public int SellerId { get; set; }

    [Required]
    public int ManufacturerId { get; set; }

    [Required]
    public int ModelId { get; set; }

    public HeadphoneKind Kind { get; set; }

    public HeadphoneConnection Connection { get; set; }

    public bool NoiseCancelling { get; set; }

    public CableConnector? Connector { get; set; }

    public bool? HasMicrophone { get; set; }

    public int? ColorId { get; set; }

    public bool HasGuarantee { get; set; }

    [StringLength(100)]
    public string? GuaranteeCorporation { get; set; }

    [DataType(DataType.Date)]
    public DateTime? GuaranteeExpiry { get; set; }

    [StringLength(500)]
    public string? GuaranteeNotes { get; set; }

    [Range(0, MoneyLimits.MaxRials)]
    public long Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ProfitPercent { get; set; }

    [Range(0, MoneyLimits.MaxRials)]
    public long? ProfitAmount { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Count must be at least 1.")]
    public int Count { get; set; } = 1;
}
