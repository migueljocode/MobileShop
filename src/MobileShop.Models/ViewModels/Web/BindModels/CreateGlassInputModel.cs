namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when creating one or more screen-protector products.</summary>
public sealed class CreateGlassInputModel
{
    [Required]
    public int ManufacturerId { get; set; }

    [Required]
    public int ModelId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, 100)]
    public decimal? ProfitPercent { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ProfitAmount { get; set; }

    [Range(1, 500, ErrorMessage = "Count must be between 1 and 500.")]
    public int Count { get; set; } = 1;
}
