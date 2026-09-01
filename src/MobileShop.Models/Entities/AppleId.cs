namespace MobileShop.Models.Entities;

[Table("AppleIds")]
public class AppleId : BaseEntity
{
	[Required, StringLength(100)]
	public string Email { get; set; } = string.Empty;

	[Required, StringLength(100)]
	public string Password { get; set; } = string.Empty;

	[StringLength(500)]
	public string? Notes { get; set; }

	[InverseProperty(nameof(Phone.AppleIdNavigation))]
	public virtual ICollection<Phone> Phones { get; set; } = [];
	// something
}
