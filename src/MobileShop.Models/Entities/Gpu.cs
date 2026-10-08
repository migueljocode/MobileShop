namespace MobileShop.Models.Entities;

[Table("Gpus")]
public class Gpu : BaseEntity
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<Laptop> Laptops { get; set; } = [];
}
