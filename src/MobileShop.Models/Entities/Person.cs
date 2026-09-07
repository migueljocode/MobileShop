namespace MobileShop.Models.Entities;

[Table("People")]
public class Person : BaseEntity
{
    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(50, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(50, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(20, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string PhoneNumber { get; set; } = string.Empty;   // named PhoneNumber, not Phone - Phone is already an entity name

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }

    // a Person can be a Seller, a Customer, and/or a User (login) all at once - e.g. the shop owner is all three
    public virtual Seller? SellerProfile { get; set; }
    public virtual Customer? CustomerProfile { get; set; }
    public virtual User? UserProfile { get; set; }
}
