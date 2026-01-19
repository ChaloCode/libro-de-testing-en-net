using Microsoft.EntityFrameworkCore;

namespace ChaloStore.Orders;

public sealed class ChaloStoreDbContext : DbContext
{
    public ChaloStoreDbContext(DbContextOptions<ChaloStoreDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
}
