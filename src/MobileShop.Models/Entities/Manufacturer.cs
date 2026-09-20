namespace MobileShop.Models.Entities;

[Table("Manufacturers")]
public class Manufacturer : BaseEntity
{
    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<Model> Models { get; set; } = [];
}