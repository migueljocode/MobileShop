namespace MobileShop.Models.Entities;

// data specific to iPhones - a sub-extension of Phone, not directly of Product
[Table("IPhones")]
public class IPhone : BaseEntity
{
    public int PhoneId { get; set; }
    public virtual Phone PhoneNavigation { get; set; } = null!;

    [Range(0, 100, ErrorMessage = "{0} must be between {1} and {2}.")]
    public int BatteryHealth { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    public int? ChargeCycle { get; set; }
}
