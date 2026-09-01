namespace MobileShop.Models.Entities;

// اجناس تعدادی (لوازم جانبی و...) - qty-based
[Table("StockGoods")]
public class StockGood : GoodBase
{
    [Column(TypeName = "decimal(18,2)")]
    public decimal SellPrice { get; set; }
    public int QtyLeft { get; set; }
    public int QtySold { get; set; }
}
