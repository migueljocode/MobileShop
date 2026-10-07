namespace MobileShop.Models.Entities;

[Table("PowerBankPorts")]
public class PowerBankPort : BaseEntity
{
    public int PowerBankId { get; set; }
    public virtual PowerBank PowerBankNavigation { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int PortNumber { get; set; }

    public CableConnector Connector { get; set; }
}
