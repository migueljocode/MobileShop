namespace MobileShop.Models.ViewModels.Web;

/// <summary>Input submitted when recording a sale.</summary>
public sealed class SellInputModel
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? Date { get; set; }
}
