namespace MobileShop.Models.Entities;

/// <summary>
/// A manufacturer part number shared by many physical phone units.
/// Belongs to exactly one <see cref="Model"/>; a Model can have many PartNumbers.
/// </summary>
[Table("PartNumbers")]
public class PartNumber : BaseEntity
{
    public int ModelId { get; set; }
    public virtual Model ModelNavigation { get; set; } = null!;

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(64, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string Code { get; set; } = string.Empty;

    public bool SupportsDualSim { get; set; }

    public bool SupportsEsim { get; set; }

    public virtual ICollection<Phone> Phones { get; set; } = [];
}
