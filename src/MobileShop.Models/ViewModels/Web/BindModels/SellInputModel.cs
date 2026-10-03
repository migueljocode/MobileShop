namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when recording a sale.</summary>
public sealed class SellInputModel
{
    [Range(1, int.MaxValue, ErrorMessage = "The product should be selected.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "The customer should be selected.")]
    public int CustomerId { get; set; }

    [Display(Name = "Finished price (IRR)")]
    [Range(0, MoneyLimits.MaxRials)]
    public long Price { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? Date { get; set; }
}
