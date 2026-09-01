namespace MobileShop.Models.Entities;

// خط خرید برای اجناس تعدادی (نه گوشی، چون گوشی خودش مستقیم به Purchase وصله)
[Table("PurchaseItems")]
public class PurchaseItem : BaseEntity
{
    public int PurchaseId { get; set; }
    [ForeignKey(nameof(PurchaseId))]
    public virtual Purchase PurchaseNavigation { get; set; } = null!;

    public int StockGoodId { get; set; }
    [ForeignKey(nameof(StockGoodId))]
    public virtual StockGood StockGoodNavigation { get; set; } = null!;

    public int Qty { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
}
