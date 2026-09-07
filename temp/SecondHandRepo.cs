namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ISecondHandRepo" />
public class SecondHandRepo(AppDbContext context) : BaseRepo<SecondHand>(context), ISecondHandRepo { }
