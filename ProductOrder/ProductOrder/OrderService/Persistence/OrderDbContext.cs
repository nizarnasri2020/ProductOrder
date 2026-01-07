using Microsoft.EntityFrameworkCore;
using OrderService.Entities;

namespace OrderService.Persistence
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<ProductCache> Products => Set<ProductCache>();
    }
}
