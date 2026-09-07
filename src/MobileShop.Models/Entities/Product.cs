namespace MobileShop.Models.Entities;

[Table("Products")]
public class Product : BaseEntity
{
    // the asking price to sell it at - discount is calculated against this
    [Range(0, double.MaxValue, ErrorMessage = "{0} cannot be negative.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Manufacturer { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Model { get; set; } = string.Empty;

    public virtual ICollection<Transaction> Transactions { get; set; } = [];

    // each is optional, depending on what kind of product this is
    public virtual AppleId? AppleIdProfile { get; set; }
    public virtual Phone? PhoneProfile { get; set; }
    public virtual SecondHand? SecondHandProfile { get; set; }
    public virtual Guarantee? GuaranteeProfile { get; set; }
}
