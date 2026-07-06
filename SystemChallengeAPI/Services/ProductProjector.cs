using Microsoft.EntityFrameworkCore;
using SystemChallengeAPI.Domain;
using SystemChallengeAPI.Infrastructure;
using SystemChallengeAPI.ReadModel;

namespace SystemChallengeAPI.Services
{
    public interface IProductProjector
    {
        Task ProjectApprovedAsync(Product product, ProductVersion version);
        Task RemoveAsync(Guid productId);
    }

    public class ProductProjector : IProductProjector
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductProjector(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ProjectApprovedAsync(Product product, ProductVersion version)
        {
            var existing = await _dbContext.ProductReadModels
                .FirstOrDefaultAsync(r => r.ProductId == product.Id);

            if (existing is null)
            {
                _dbContext.ProductReadModels.Add(new ProductReadModel
                {
                    ProductId = product.Id,
                    Name = version.Name,
                    Description = version.Description,
                    Price = version.Price,
                    Sku = version.Sku,
                    VersionNumber = version.VersionNumber,
                    VersionId = version.Id,
                    ApprovedBy = version.DecidedBy ?? string.Empty,
                    ApprovedAt = version.DecidedAt ?? DateTime.UtcNow
                });
            }
            else
            {
                existing.Name = version.Name;
                existing.Description = version.Description;
                existing.Price = version.Price;
                existing.Sku = version.Sku;
                existing.VersionNumber = version.VersionNumber;
                existing.VersionId = version.Id;
                existing.ApprovedBy = version.DecidedBy ?? string.Empty;
                existing.ApprovedAt = version.DecidedAt ?? DateTime.UtcNow;
            }

        }

        public async Task RemoveAsync(Guid productId)
        {
            var existing = await _dbContext.ProductReadModels
                .FirstOrDefaultAsync(r => r.ProductId == productId);

            if (existing is not null)
                _dbContext.ProductReadModels.Remove(existing);
        }
    }
}