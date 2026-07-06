using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SystemChallengeAPI.Domain;
using SystemChallengeAPI.DTOs;
using SystemChallengeAPI.Infrastructure;

namespace SystemChallengeAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest req, string createdBy)
        {
            var now = DateTime.UtcNow;

            var version = new ProductVersion
            {
                Id = Guid.NewGuid(),
                VersionNumber = 1,
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Sku = req.Sku,
                Status = WorkflowStatus.Draft,
                CreatedBy = createdBy,
                CreatedAt = now
            };

            var product = new Product
            {
                Id = Guid.NewGuid(),
                IsDeleted = false,
                CreatedBy = createdBy,
                CreatedAt = now,
                CurrentApprovedVersionId = null,
                Versions = new List<ProductVersion> { version }
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            return MapToResponse(product, version);
        }

        public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest req, string createdBy)
        {

            var product = await GetProductByIdAsync(id);
            if (product == null)
                return null;

            var now = DateTime.UtcNow;

            var version = new ProductVersion
            {
                Id = Guid.NewGuid(),
                ProductId = id,
                VersionNumber = product.Versions.Count + 1,
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Sku = req.Sku,
                Status = WorkflowStatus.Draft,
                CreatedBy = createdBy,
                CreatedAt = now
            };

            _dbContext.ProductVersions.Add(version);
            await _dbContext.SaveChangesAsync();

            return MapToResponse(product, version);
        }

        /* 
         * Get Product and the approved version or latest version if none approved 
         */
        public async Task<ProductResponse> GetByIdAsync(Guid id)
        {
            var product = await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Versions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return null;

            var version = product.Versions.FirstOrDefault(v => v.Id == product.CurrentApprovedVersionId);
            if (version == null)
            {
                version = await GetLatestProductVersion(product);
            }

            return MapToResponse(product, version);
        }

        // Change returns
        public async Task<ProductResponse> SubmitVersionForReview(Guid productId, Guid versionId, string submittedBy)
        {

            var product = await GetTrackedProductAsync(productId);
            if (product == null)
                return null;

            var version = product.Versions.FirstOrDefault(v => v.Id == versionId);
            if (version == null)
                return null;
            if (version.CreatedBy != submittedBy)
                return null;

            version.Status = WorkflowStatus.Pending;

            await _dbContext.SaveChangesAsync();

            return MapToResponse(product, version);
        }

        public async Task<ProductResponse> ApproveProductVersion(WorkflowStatusChangeRequest workflowStatusChangeRequest, string approvedBy)
        {
            var product = await GetTrackedProductAsync(workflowStatusChangeRequest.productId);
            if (product == null)
                return null;

            var version = product.Versions.FirstOrDefault(v => v.Id == workflowStatusChangeRequest.versionId);
            if (version == null)
                return null;
            if (version.CreatedBy == approvedBy)
                return null;

            version.Status = WorkflowStatus.Approved;
            version.DecidedBy = approvedBy;
            version.DecidedAt = DateTime.UtcNow;
            version.DecisionReason = workflowStatusChangeRequest.DecisionReason;

            product.CurrentApprovedVersionId = version.Id;

            await _dbContext.SaveChangesAsync();

            return MapToResponse(product, version);
        }

        public async Task<ProductResponse> RejectProductVersion(WorkflowStatusChangeRequest workflowStatusChangeRequest, string rejectedBy)
        {
            var product = await GetTrackedProductAsync(workflowStatusChangeRequest.productId);
            if (product == null)
                return null;

            var version = product.Versions.FirstOrDefault(v => v.Id == workflowStatusChangeRequest.versionId);
            if (version == null)
                return null;
            if (version.CreatedBy == rejectedBy)
                return null;

            version.Status = WorkflowStatus.Rejected;
            version.DecidedBy = rejectedBy;
            version.DecidedAt = DateTime.UtcNow;
            version.DecisionReason = workflowStatusChangeRequest.DecisionReason;

            await _dbContext.SaveChangesAsync();

            return MapToResponse(product, version);
        }

        private async Task<ProductVersion> GetLatestProductVersion(Product product)
        {
            var version = product.Versions
                    .OrderByDescending(v => v.VersionNumber)
                    .FirstOrDefault();

            return version;
        }

        /* 
         * Get Product and all versions
         */
        private async Task<Product> GetProductByIdAsync(Guid id)
        {
            var product = await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Versions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return null;

            return product;
        }

        private async Task<Product?> GetTrackedProductAsync(Guid id)
        {
            return await _dbContext.Products
                .Include(p => p.Versions)
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        private static ProductResponse MapToResponse(Product product, ProductVersion version)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = version.Name,
                Description = version.Description,
                Price = version.Price,
                Sku = version.Sku,
                CurrentVersionId = version.Id,
                Status = version.Status
            };
        }
    }
}
