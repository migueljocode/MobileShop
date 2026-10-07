namespace MobileShop.Models.ViewModels.Web.BindModels;

public sealed class CreatePortableStorageInputModel
{
    [Required] public int ManufacturerId { get; set; }
    [Required] public int ModelId { get; set; }
    public StorageKind StorageKind { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Select a storage capacity.")] public int StorageCapacityId { get; set; }
    [Range(0, int.MaxValue)] public int? Speed { get; set; }
    [Range(0, MoneyLimits.MaxRials)] public long Price { get; set; }
    [Range(0, double.MaxValue)] public decimal? ProfitPercent { get; set; }
    [Range(0, MoneyLimits.MaxRials)] public long? ProfitAmount { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Count must be at least 1.")] public int Count { get; set; } = 1;
}
