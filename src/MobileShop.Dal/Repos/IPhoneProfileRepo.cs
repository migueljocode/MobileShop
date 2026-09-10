namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IIPhoneProfileRepo" />
public class IPhoneProfileRepo(AppDbContext context) : BaseRepo<IPhone>(context), IIPhoneProfileRepo { }