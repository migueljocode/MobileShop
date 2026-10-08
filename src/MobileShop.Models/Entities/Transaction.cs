namespace MobileShop.Models.Entities;

[Table("Transactions")]
public class Transaction : BaseEntity
{
    // bounded to a sane real-world window - guards against default/garbage DateTime values (e.g. DateTime.MinValue) and far-future typos
    [Range(typeof(DateTime), "2000-01-01", "2100-01-01", ErrorMessage = "{0} must be between {1} and {2}.")]
    public DateTime Date { get; set; }

    // Both parties are required. For a purchase, the seller supplies the product and the customer is the buyer.
    public int SellerId { get; set; }
    public virtual Seller SellerNavigation { get; set; } = null!;

    public int CustomerId { get; set; }
    public virtual Customer CustomerNavigation { get; set; } = null!;

    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    [Range(0, long.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public long FinishedPrice { get; set; }

    public TransactionDirection Direction { get; set; }
}
