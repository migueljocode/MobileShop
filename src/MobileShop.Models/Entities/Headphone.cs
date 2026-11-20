namespace MobileShop.Models.Entities;

[Table("Headphones")]
public class Headphone : BaseEntity
{
    public int ProductId { get; set; }
    public virtual Product ProductNavigation { get; set; } = null!;

    public HeadphoneKind Kind { get; set; }
    public HeadphoneConnection Connection { get; set; }
    public bool NoiseCancelling { get; set; }
    public CableConnector? Connector { get; set; }
    public bool? HasMicrophone { get; set; }

    [StringLength(500, ErrorMessage = "{0} cannot exceed {1} characters.")]
    public string? Notes { get; set; }
}
