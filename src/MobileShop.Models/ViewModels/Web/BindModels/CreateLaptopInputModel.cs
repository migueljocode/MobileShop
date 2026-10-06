namespace MobileShop.Models.ViewModels.Web.BindModels;

public sealed class CreateLaptopInputModel
{
    [Required] public int ManufacturerId { get; set; }
    [Required] public int ModelId { get; set; }
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
    [Required, StringLength(100)] public string Gpu { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal DisplaySize { get; set; }
}
