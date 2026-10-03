namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when creating one or more screen-protector products.</summary>
public sealed class CreateGlassInputModel
{
    [Required]
    public int CompatibleManufacturerId { get; set; }

    [Required]
    public int GlassManufacturerId { get; set; }

    [Required]
    public int CompatibleModelId { get; set; }

    [Range(0, MoneyLimits.MaxRials)]
    public long Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ProfitPercent { get; set; }

    [Range(0, MoneyLimits.MaxRials)]
    public long? ProfitAmount { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Count must be at least 1.")]
    public int Count { get; set; } = 1;
}