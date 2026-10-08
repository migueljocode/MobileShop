namespace MobileShop.Models.ViewModels.Web.BindModels;

public sealed class CreateChargerInputModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a seller.")]
    public int SellerId { get; set; }
    [Required] public int ManufacturerId { get; set; }
    [Required] public int ModelId { get; set; }
    [Range(0, int.MaxValue)] public int Wattage { get; set; }
    public bool Pd { get; set; }
    [Range(1, int.MaxValue)] public int PortCount { get; set; } = 1;
    [Range(0, MoneyLimits.MaxRials)] public long Price { get; set; }
    [Range(0, double.MaxValue)] public decimal? ProfitPercent { get; set; }
    [Range(0, MoneyLimits.MaxRials)] public long? ProfitAmount { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Count must be at least 1.")] public int Count { get; set; } = 1;
}
