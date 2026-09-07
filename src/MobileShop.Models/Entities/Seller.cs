namespace MobileShop.Models.Entities;

[Table("Sellers")]
public class Seller : BaseEntity
{
    public int PersonId { get; set; }
    public virtual Person PersonNavigation { get; set; } = null!;

    public SellerEntityType EntityType { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = [];
}
