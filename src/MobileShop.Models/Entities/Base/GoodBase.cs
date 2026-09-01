namespace MobileShop.Models.Entities.Base;

public abstract class GoodBase : BaseEntity, IGood
{
	[Required, StringLength(100)]
	public string Name { get; set; } = string.Empty;

	[Column(TypeName = "decimal(18,2)")]
	public decimal PurchasePrice { get; set; }

	//    something special
}
