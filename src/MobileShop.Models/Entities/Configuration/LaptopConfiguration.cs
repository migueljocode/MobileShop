namespace MobileShop.Models.Entities.Configuration;

public class LaptopConfiguration : IEntityTypeConfiguration<Laptop>
{
    public void Configure(EntityTypeBuilder<Laptop> builder)
    {
        builder.HasOne(l => l.ProductNavigation)
            .WithOne(product => product.LaptopProfile)
            .HasForeignKey<Laptop>(l => l.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(l => l.CpuNavigation)
            .WithMany(cpu => cpu.Laptops)
            .HasForeignKey(l => l.CpuId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(l => l.GpuNavigation)
            .WithMany(gpu => gpu.Laptops)
            .HasForeignKey(l => l.GpuId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
