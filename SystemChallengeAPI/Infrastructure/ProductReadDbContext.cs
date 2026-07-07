using Microsoft.EntityFrameworkCore;
using SystemChallengeAPI.ReadModel;

namespace SystemChallengeAPI.Infrastructure
{
    public class ProductReadDbContext : DbContext
    {
        public ProductReadDbContext(DbContextOptions<ProductReadDbContext> options) : base(options)
        {
        }

        public DbSet<ProductReadModel> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductReadModel>(readModel =>
            {
                readModel.ToTable("ProductReadModel");
                readModel.HasKey(r => r.ProductId);
            });
        }
    }
}