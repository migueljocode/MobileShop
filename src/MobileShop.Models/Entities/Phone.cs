namespace MobileShop.Models.Entities;

public enum PhoneStatus { InStock, Sold }

// گوشی - سریالی، هر رکورد یک دستگاه واقعی با IMEI مخصوص خودش
// یونیک بودن IMEI1 توی PhoneConfiguration به‌صورت filtered index تنظیم شده (فقط رکوردهای غیرحذف‌شده)
[Table("Phones")]
public class Phone : GoodBase
{
    [Required, StringLength(20)]
    public string IMEI1 { get; set; } = string.Empty;

    [StringLength(20)]
    public string? IMEI2 { get; set; }

    public bool IsNew { get; set; }

    [StringLength(200)]
    public string? Warranty { get; set; }

    public int? TestPeriodDays { get; set; }          // فقط دستگاه کارکرده

    [Column(TypeName = "decimal(18,2)")]
    public decimal ListPrice { get; set; }             // قیمت بدون تخفیف

    [Column(TypeName = "decimal(18,2)")]
    public decimal? SoldPrice { get; set; }             // بعد از فروش پر می‌شود

    public PhoneStatus Status { get; set; } = PhoneStatus.InStock;
    public bool OwnershipTransferred { get; set; }

    public int PurchaseId { get; set; }
    [ForeignKey(nameof(PurchaseId))]
    public virtual Purchase PurchaseNavigation { get; set; } = null!;

    public int? SaleId { get; set; }
    [ForeignKey(nameof(SaleId))]
    public virtual Sale? SaleNavigation { get; set; }

    public int? CustomerId { get; set; }
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? CustomerNavigation { get; set; }

    public int? AppleIdId { get; set; }
    [ForeignKey(nameof(AppleIdId))]
    public virtual AppleId? AppleIdNavigation { get; set; }
}
