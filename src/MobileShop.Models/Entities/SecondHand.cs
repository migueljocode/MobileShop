namespace MobileShop.Models.Entities;

// not just phones - iPads, Macs, laptops can be secondhand too, so this relates to Product, not Phone
[Table("SecondHands")]
public class SecondHand : BaseEntity
{
    [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public int? TestPeriodDays { get; set; }

    // total number of days the product has already been in use - the years/months/days display helper
    // now lives in MobileShop.Dal.Extensions.SecondHandExtensions, kept out of here to keep this a plain data shape
    [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public int? UsedDurationDays { get; set; }

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }

    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;
}
