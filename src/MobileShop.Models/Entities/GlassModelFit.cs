namespace MobileShop.Models.Entities;

/// <summary>
/// Explicit many-to-many join between <see cref="Glass"/> and <see cref="Model"/>: which device
/// models a given screen protector fits.
/// </summary>
[Table("GlassModelFits")]
public class GlassModelFit : BaseEntity
{
    public int GlassId { get; set; }
    public virtual Glass GlassNavigation { get; set; } = null!;

    public int ModelId { get; set; }
    public virtual Model ModelNavigation { get; set; } = null!;
}