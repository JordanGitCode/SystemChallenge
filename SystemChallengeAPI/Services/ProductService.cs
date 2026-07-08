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
        private readonly IProductProjector _projector;

        public ProductService(ApplicationDbContext dbContext, IProductProjector projector)
        {
            _dbContext = dbContext;
            _projector = projector;
        }

        public async Task<OperationResult<ProductResponse>> CreateAsync(CreateProductRequest req, string createdBy)
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

            return OperationResult<ProductResponse>.Ok(MapToResponse(product, version));
        }

        public async Task<OperationResult<ProductResponse>> UpdateAsync(Guid id, UpdateProductRequest req, string createdBy)
        {

            var product = await GetProductByIdAsync(id);
            if (product == null)
                return OperationResult<ProductResponse>.NotFound("Product not found");

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

            return OperationResult<ProductResponse>.Ok(MapToResponse(product, version));
        }

        /* 
         * Get Product and the approved version or latest version if none approved 
         */
        public async Task<OperationResult<ProductResponse>> GetByIdAsync(Guid id)
        {
            var product = await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Versions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return OperationResult<ProductResponse>.NotFound("Product not found");

            var version = product.Versions.FirstOrDefault(v => v.Id == product.CurrentApprovedVersionId);
            if (version == null)
            {
                version = GetLatestProductVersion(product);
            }
            if (version == null)
            {
                return OperationResult<ProductResponse>.NotFound("No versions found");
            }

            return OperationResult<ProductResponse>.Ok(MapToResponse(product, version));
        }

        public async Task<List<ProductResponse>> GetAllProductsAsync()
        {
            var products = await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Versions)
                .ToListAsync();

            List<ProductResponse> productResponses = new List<ProductResponse>();

            foreach (var product in products)
            {
                var version = GetVerifiedProductVersion(product);
                if (version == null)
                    continue;

                productResponses.Add(MapToResponse(product, version));
            }

            return productResponses;
        }

        public async Task<OperationResult<ProductResponse>> SubmitVersionForReview(Guid productId, Guid versionId, string submittedBy)
        {
            var product = await GetTrackedProductAsync(productId);
            if (product is null)
                return OperationResult<ProductResponse>.NotFound("Product not found.");

            var version = product.Versions.FirstOrDefault(v => v.Id == versionId);
            if (version is null)
                return OperationResult<ProductResponse>.NotFound("Version not found.");

            if (version.CreatedBy != submittedBy)
                return OperationResult<ProductResponse>.Forbidden("You can only submit your own version.");

            if (version.Status != WorkflowStatus.Draft)
                return OperationResult<ProductResponse>.Invalid($"Cannot submit a version in '{version.Status}' state. It must be Draft.");

            version.Status = WorkflowStatus.Pending;

            await _dbContext.SaveChangesAsync();
            return OperationResult<ProductResponse>.Ok(MapToResponse(product, version));
        }

        public async Task<OperationResult<ProductResponse>> ApproveProductVersion(WorkflowStatusChangeRequest workflowStatusChangeRequest, string approvedBy)
        {
            var product = await GetTrackedProductAsync(workflowStatusChangeRequest.productId);
            if (product == null)
                return OperationResult<ProductResponse>.NotFound("Product not found");

            var version = product.Versions.FirstOrDefault(v => v.Id == workflowStatusChangeRequest.versionId);
            if (version == null)
                return OperationResult<ProductResponse>.NotFound("Version not found");
            if (version.CreatedBy == approvedBy)
                return OperationResult<ProductResponse>.Forbidden("You cannot approve a product that you authored");
            if (version.Status != WorkflowStatus.Pending)
                return OperationResult<ProductResponse>.Invalid("You cannot approve a product that is not in the pending state");

            version.Status = WorkflowStatus.Approved;
            version.DecidedBy = approvedBy;
            version.DecidedAt = DateTime.UtcNow;
            version.DecisionReason = workflowStatusChangeRequest.DecisionReason;

            product.CurrentApprovedVersionId = version.Id;

            await _projector.ProjectApprovedAsync(product, version);
            await _dbContext.SaveChangesAsync();

            return OperationResult<ProductResponse>.Ok(MapToResponse(product, version));
        }

        public async Task<OperationResult<ProductResponse>> RejectProductVersion(WorkflowStatusChangeRequest workflowStatusChangeRequest, string rejectedBy)
        {
            var product = await GetTrackedProductAsync(workflowStatusChangeRequest.productId);
            if (product == null)
                return OperationResult<ProductResponse>.NotFound("Product not found");

            var version = product.Versions.FirstOrDefault(v => v.Id == workflowStatusChangeRequest.versionId);
            if (version == null)
                return OperationResult<ProductResponse>.NotFound("Version not found");
            if (version.CreatedBy == rejectedBy)
                return OperationResult<ProductResponse>.Forbidden("You cannot reject a product that you authored");
            if (version.Status != WorkflowStatus.Pending)
                return OperationResult<ProductResponse>.Invalid("You cannot reject a product that is not in the pending state");

            version.Status = WorkflowStatus.Rejected;
            version.DecidedBy = rejectedBy;
            version.DecidedAt = DateTime.UtcNow;
            version.DecisionReason = workflowStatusChangeRequest.DecisionReason;

            await _dbContext.SaveChangesAsync();

            return OperationResult<ProductResponse>.Ok(MapToResponse(product, version));
        }

        public async Task<OperationResult<ProductResponse>> SoftDeleteAsync(Guid id, string deletedBy)
        {
            var product = await GetTrackedProductAsync(id);
            if (product is null)
                return OperationResult<ProductResponse>.NotFound("Product not found.");

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            product.DeletedBy = deletedBy;

            await _projector.RemoveAsync(id);
            await _dbContext.SaveChangesAsync();

            return OperationResult<ProductResponse>.Ok(null!);
        }

        public async Task<OperationResult<ProductResponse>> RestoreAsync(Guid id)
        {
            var product = await _dbContext.Products
                .IgnoreQueryFilters()
                .Include(p => p.Versions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return OperationResult<ProductResponse>.NotFound("Product not found.");

            if (!product.IsDeleted)
                return OperationResult<ProductResponse>.Invalid("Cannot restore a product that is not deleted");

            product.IsDeleted = false;
            product.DeletedAt = null;
            product.DeletedBy = null;

            if (product.CurrentApprovedVersionId != null)
            {
                var approved = product.Versions.First(v => v.Id == product.CurrentApprovedVersionId);
                await _projector.ProjectApprovedAsync(product, approved);
            }

            await _dbContext.SaveChangesAsync();

            return OperationResult<ProductResponse>.Ok(null!);
        }

        private static ProductVersion? GetLatestProductVersion(Product product)
        {
            return product.Versions
                    .OrderByDescending(v => v.VersionNumber)
                    .FirstOrDefault();
        }

        private static ProductVersion? GetVerifiedProductVersion(Product product)
        {
            var version = product.Versions.FirstOrDefault(v => v.Id == product.CurrentApprovedVersionId);
            if (version == null)
                version = product.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();

            return version;
        }

        /* 
         * Get Product and all versions
         */
        private async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Versions)
                .FirstOrDefaultAsync(p => p.Id == id);
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
