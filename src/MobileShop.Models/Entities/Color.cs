namespace MobileShop.Models.Entities;

[Table("Colors")]
public class Color : BaseEntity
{
    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(50, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<Product> Products { get; set; } = [];
}