using Microsoft.EntityFrameworkCore;
using ProductService.Entities;

namespace ProductService.Persistence
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options) { }

        public DbSet<Product> Products => Set<Product>();
    }
}
