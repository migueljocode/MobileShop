namespace MobileShop.Models.Entities;

/// <summary>
/// Explicit many-to-many join between <see cref="Case"/> and <see cref="Model"/>: which device
/// models a given case fits.
/// </summary>
[Table("CaseModelFits")]
public class CaseModelFit : BaseEntity
{
    public int CaseId { get; set; }
    public virtual Case CaseNavigation { get; set; } = null!;

    public int ModelId { get; set; }
    public virtual Model ModelNavigation { get; set; } = null!;
}