namespace MobileShop.Models.Entities;

[Table("Products")]
public class Product : BaseEntity
{
    // the asking price to sell it at - discount is calculated against this
    [Range(0, double.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    // the manufacturer and the category are both reached through the model - a model's category
    // is a permanent fact about that model, so neither is duplicated on the product
    public int ModelId { get; set; }
    public virtual Model ModelNavigation { get; set; } = null!;

    public int? ColorId { get; set; }
    public virtual Color? ColorNavigation { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(64, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Barcode { get; set; } = string.Empty;

    public virtual ICollection<Transaction> Transactions { get; set; } = [];

    // each is optional, depending on what kind of product this is
    public virtual AppleId? AppleIdProfile { get; set; }
    public virtual Phone? PhoneProfile { get; set; }
    public virtual SecondHand? SecondHandProfile { get; set; }
    public virtual Guarantee? GuaranteeProfile { get; set; }
    public virtual DeviceSpec? DeviceSpecProfile { get; set; }
    public virtual AppleInfo? AppleInfoProfile { get; set; }
    public virtual Tablet? TabletProfile { get; set; }
    public virtual SmartWatch? SmartWatchProfile { get; set; }
    public virtual Laptop? LaptopProfile { get; set; }
    public virtual Cable? CableProfile { get; set; }
    public virtual Charger? ChargerProfile { get; set; }
    public virtual PowerBank? PowerBankProfile { get; set; }
    public virtual PortableStorage? PortableStorageProfile { get; set; }
    public virtual Case? CaseProfile { get; set; }
    public virtual Glass? GlassProfile { get; set; }
}
