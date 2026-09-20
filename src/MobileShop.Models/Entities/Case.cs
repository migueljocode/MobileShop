namespace MobileShop.Models.Entities;

/// <summary>
/// A phone case. One-to-one with <see cref="Product"/>; the models it fits are listed in
/// <see cref="ModelFits"/> (an explicit many-to-many join).
/// </summary>
[Table("Cases")]
public class Case : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }

    public virtual ICollection<CaseModelFit> ModelFits { get; set; } = [];
}