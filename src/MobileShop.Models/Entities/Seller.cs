namespace MobileShop.Models.Entities;

[Table("Sellers")]
public class Seller : BaseEntity
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;   // Phone قبلاً برای انتیتی گوشی گرفته شده

    [StringLength(500)]
    public string? Notes { get; set; }

    [InverseProperty(nameof(Purchase.SellerNavigation))]
    public virtual ICollection<Purchase> Purchases { get; set; } = [];
}
