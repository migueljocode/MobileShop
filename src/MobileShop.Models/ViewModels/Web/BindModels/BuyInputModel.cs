namespace MobileShop.Models.ViewModels.Web.BindModels;

/// <summary>Input submitted when recording a purchase.</summary>
public sealed class BuyInputModel
{
    [Range(1, int.MaxValue, ErrorMessage = "The product should be selected.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "The seller should be selected.")]
    public int SellerId { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Finished price")]
    public decimal Price { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? Date { get; set; }
}
