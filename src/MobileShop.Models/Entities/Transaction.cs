namespace MobileShop.Models.Entities;

[Table("Transactions")]
public class Transaction : BaseEntity
{
    // bounded to a sane real-world window - guards against default/garbage DateTime values (e.g. DateTime.MinValue) and far-future typos
    [Range(typeof(DateTime), "2000-01-01", "2100-01-01", ErrorMessage = "{0} must be between {1} and {2}.")]
    public DateTime Date { get; set; }

    // both are always set - for a purchase leg customerId is the shop itself, for a sale leg sellerId is the shop itself
    public int SellerId { get; set; }
    public virtual Seller SellerNavigation { get; set; } = null!;

    public int CustomerId { get; set; }
    public virtual Customer CustomerNavigation { get; set; } = null!;

    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    [Range(0, double.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal FinishedPrice { get; set; }

    public TransactionDirection Direction { get; set; }
}
