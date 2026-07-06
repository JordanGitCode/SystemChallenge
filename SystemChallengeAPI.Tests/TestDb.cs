using Microsoft.EntityFrameworkCore;
using SystemChallengeAPI.Domain;
using SystemChallengeAPI.Infrastructure;
using SystemChallengeAPI.Services;

public static class TestDb
{
    public static ApplicationDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    public static async Task<(Product product, ProductVersion version)> SeedProduct(ApplicationDbContext ctx, WorkflowStatus status, string createdBy)
    {
        var version = new ProductVersion
        {
            Id = Guid.NewGuid(),
            VersionNumber = 1,
            Name = "Test",
            Description = "d",
            Price = 1m,
            Sku = "SKU-1",
            Status = status,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            Versions = new List<ProductVersion> { version }
        };

        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();
        return (product, version);
    }

    public static async Task<ProductVersion> AddVersion(ApplicationDbContext ctx, Guid productId, int versionNumber,
        WorkflowStatus status, string createdBy, string name)
    {
        var version = new ProductVersion
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            VersionNumber = versionNumber,
            Name = name,
            Description = "d",
            Price = 2m,
            Sku = "SKU-" + versionNumber,
            Status = status,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        ctx.ProductVersions.Add(version);
        await ctx.SaveChangesAsync();
        return version;
    }

    public static ProductService NewService(ApplicationDbContext ctx)
    {
        return new ProductService(ctx, new ProductProjector(ctx));
    }
}