namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="IPersonRepo" />
public class PersonRepo(AppDbContext context) : BaseRepo<Person>(context), IPersonRepo { }
