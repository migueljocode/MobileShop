namespace MobileShop.Dal.Repos;

/// <inheritdoc cref="ISellerRepo" />
public class SellerRepo(AppDbContext context) : BaseRepo<Seller>(context), ISellerRepo { }
