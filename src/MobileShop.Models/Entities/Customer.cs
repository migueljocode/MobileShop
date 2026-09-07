namespace MobileShop.Models.Entities;

[Table("Customers")]
public class Customer : BaseEntity
{
    public int PersonId { get; set; }
    public virtual Person PersonNavigation { get; set; } = null!;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(10, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string NationalId { get; set; } = string.Empty;

    public virtual ICollection<Transaction> Transactions { get; set; } = [];
}
