namespace MobileShop.Models.Entities;

[Table("SaleItems")]
public class SaleItem : BaseEntity
{
    public int SaleId { get; set; }
    [ForeignKey(nameof(SaleId))]
    public virtual Sale SaleNavigation { get; set; } = null!;

    public int StockGoodId { get; set; }
    [ForeignKey(nameof(StockGoodId))]
    public virtual StockGood StockGoodNavigation { get; set; } = null!;

    public int Qty { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
}
