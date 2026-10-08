namespace MobileShop.Models.Entities;

[Table("Cpus")]
public class Cpu : BaseEntity
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<Laptop> Laptops { get; set; } = [];
}
