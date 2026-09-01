namespace MobileShop.Models.Entities;

[Table("Purchases")]
public class Purchase : BaseEntity
{
    public DateTime Date { get; set; }

    public int SellerId { get; set; }
    [ForeignKey(nameof(SellerId))]
    public virtual Seller SellerNavigation { get; set; } = null!;

    [InverseProperty(nameof(Phone.PurchaseNavigation))]
    public virtual ICollection<Phone> Phones { get; set; } = [];

    [InverseProperty(nameof(PurchaseItem.PurchaseNavigation))]
    public virtual ICollection<PurchaseItem> Items { get; set; } = [];   // برای StockGood
}
