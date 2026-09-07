namespace MobileShop.Models.Entities.Configuration;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // defense in depth - Range on the property is only enforced by app-level validation (e.g. ModelState);
        // this enforces the same rule at the database level regardless of how a row gets written
        builder.ToTable(t => t.HasCheckConstraint("CK_Transactions_FinishedPrice_NonNegative", "FinishedPrice >= 0"));

        builder.HasOne(t => t.SellerNavigation)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.SellerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.CustomerNavigation)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.ProductNavigation)
            .WithMany(p => p.Transactions)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
