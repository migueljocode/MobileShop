namespace MobileShop.Models.Entities;

[Table("Customers")]
public class Customer : BaseEntity
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(10)]
    public string NationalId { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [InverseProperty(nameof(Phone.CustomerNavigation))]
    public virtual ICollection<Phone> PurchasedPhones { get; set; } = [];
}
