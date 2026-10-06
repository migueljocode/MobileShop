namespace MobileShop.Models.ViewModels.Web.BindModels;

public sealed class CreateCaseInputModel
{
    [Required] public int ManufacturerId { get; set; }
    [Required] public int CompatibleManufacturerId { get; set; }
    public List<int> CompatibleModelIds { get; set; } = [];
    [Range(0, MoneyLimits.MaxRials)] public long Price { get; set; }
    [Range(0, double.MaxValue)] public decimal? ProfitPercent { get; set; }
    [Range(0, MoneyLimits.MaxRials)] public long? ProfitAmount { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Count must be at least 1.")] public int Count { get; set; } = 1;
}
