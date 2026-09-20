namespace MobileShop.Models.Entities;

/// <summary>
/// A model of a device, as produced by a manufacturer. A model's <see cref="CategoryId"/> is a
/// permanent fact about the model itself - it does not vary per physical unit sold, which is why
/// the category lives here and not on <see cref="Product"/>.
/// </summary>
[Table("Models")]
public class Model : BaseEntity
{
    public int ManufacturerId { get; set; }
    public virtual Manufacturer ManufacturerNavigation { get; set; } = null!;

    public int CategoryId { get; set; }
    public virtual Category CategoryNavigation { get; set; } = null!;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(100, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? ModelNumber { get; set; }

    public virtual ICollection<Product> Products { get; set; } = [];
    public virtual ICollection<CaseModelFit> CaseFits { get; set; } = [];
    public virtual ICollection<GlassModelFit> GlassFits { get; set; } = [];
}