namespace MobileShop.Models.Entities;

/// <summary>
/// A screen protector (glass). One-to-one with <see cref="Product"/>; the models it fits are
/// listed in <see cref="ModelFits"/> (an explicit many-to-many join).
/// </summary>
/// <remarks>
/// Services - not the schema - enforce that every linked model's
/// <see cref="Model.CategoryNavigation"/> name is one of Phone, Tablet or SmartWatch. This mirrors
/// the "Apple-only" enforcement style used for <see cref="AppleInfo"/>.
/// </remarks>
[Table("Glasses")]
public class Glass : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }

    public virtual ICollection<GlassModelFit> ModelFits { get; set; } = [];
}