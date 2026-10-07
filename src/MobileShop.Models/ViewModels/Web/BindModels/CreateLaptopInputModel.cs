namespace MobileShop.Models.ViewModels.Web.BindModels;

public sealed class CreateLaptopInputModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a manufacturer.")]
    public int ManufacturerId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Select a model.")]
    public int ModelId { get; set; }
    [Range(0, MoneyLimits.MaxRials)] public long Price { get; set; }
    [Range(0, 100)] public decimal? ProfitPercent { get; set; }
    [Range(0, MoneyLimits.MaxRials)] public long? ProfitAmount { get; set; }
    public bool IsSecondHand { get; set; }
    [Range(0, int.MaxValue)] public int? TestPeriodDays { get; set; }
    [StringLength(500)] public string? SecondHandNotes { get; set; }
    public bool HasGuarantee { get; set; }
    [StringLength(100)] public string? GuaranteeCorporation { get; set; }
    [DataType(DataType.Date)] public DateTime? GuaranteeExpiry { get; set; }
    [StringLength(500)] public string? GuaranteeNotes { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
    [Required, StringLength(100)] public string Cpu { get; set; } = string.Empty;
    [StringLength(100)] public string? Gpu { get; set; }
    [Range(0.1, double.MaxValue, ErrorMessage = "Display size must be greater than 0.")] public decimal DisplaySize { get; set; }
}
