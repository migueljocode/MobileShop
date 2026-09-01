namespace MobileShop.Models.Entities;

[Table("Sales")]
public class Sale : BaseEntity
{
    public DateTime Date { get; set; }

    [InverseProperty(nameof(Phone.SaleNavigation))]
    public virtual ICollection<Phone> Phones { get; set; } = [];

    [InverseProperty(nameof(SaleItem.SaleNavigation))]
    public virtual ICollection<SaleItem> Items { get; set; } = [];   // برای StockGood
}
