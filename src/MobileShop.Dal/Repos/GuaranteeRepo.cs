namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IGuaranteeRepo" />
public class GuaranteeRepo(AppDbContext context) : BaseRepo<Guarantee>(context), IGuaranteeRepo { }
