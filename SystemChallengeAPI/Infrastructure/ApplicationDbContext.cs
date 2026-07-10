using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using SystemChallengeAPI.Domain;
using SystemChallengeAPI.ReadModel;

namespace SystemChallengeAPI.Infrastructure;

public class ApplicationDbContext : DbContext
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<Product> Products {  get; set; }
    public DbSet<ProductVersion> ProductVersions { get; set; }
    public DbSet<ProductReadModel> ProductReadModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Product>(product =>
        {
            product.HasKey(e => e.Id);

            product.HasQueryFilter(p => !p.IsDeleted);

            product.Property(p => p.CreatedBy).HasMaxLength(256);
            product.Property(p => p.DeletedBy).HasMaxLength(256);

            product.HasMany(p => p.Versions)
                 .WithOne()
                 .HasForeignKey(v => v.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);

            product.HasOne<ProductVersion>()
                 .WithMany()
                 .HasForeignKey(p => p.CurrentApprovedVersionId)
                 .OnDelete(DeleteBehavior.NoAction);

            product.Navigation(p => p.Versions);
            product.UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ProductVersion>(productVersion =>
        {
            productVersion.HasKey(e => e.Id);

            productVersion.HasIndex(v => new { v.ProductId, v.VersionNumber }).IsUnique();

            productVersion.Property(v => v.Name).HasMaxLength(200).IsRequired();
            productVersion.Property(v => v.Sku).HasMaxLength(64).IsRequired();
            productVersion.Property(v => v.Price).HasPrecision(18, 2);
            productVersion.Property(v => v.CreatedBy).HasMaxLength(256);
            productVersion.Property(v => v.DecidedBy).HasMaxLength(256);

            productVersion.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<ProductReadModel>(readModel =>
        {
            readModel.ToTable("ProductReadModel");
            readModel.HasKey(r => r.ProductId);

            readModel.Property(r => r.Sequence).UseIdentityColumn();
            readModel.HasIndex(r => r.Sequence).IsUnique();

            readModel.Property(r => r.Name).HasMaxLength(200);
            readModel.Property(r => r.Sku).HasMaxLength(64);
            readModel.Property(r => r.Price).HasPrecision(18, 2);
            readModel.Property(r => r.ApprovedBy).HasMaxLength(256);

            readModel.HasIndex(r => r.Sku);
        });

        SeedData(modelBuilder);
    }

    // AI Generated seed data:
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Fixed identifiers
        var productA = new Guid("11111111-1111-1111-1111-111111111111"); // live product
        var productB = new Guid("22222222-2222-2222-2222-222222222222"); // live + edit in flight
        var productC = new Guid("33333333-3333-3333-3333-333333333333"); // rejected, being reworked

        const string capturer = "capturer@moyo.com";
        const string manager = "manager@moyo.com";

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = productA, CreatedBy = capturer, CreatedAt = new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new Product { Id = productB, CreatedBy = capturer, CreatedAt = new DateTime(2026, 5, 3, 8, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new Product { Id = productC, CreatedBy = capturer, CreatedAt = new DateTime(2026, 5, 5, 8, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );

        modelBuilder.Entity<ProductVersion>().HasData(
            // Product A — a single approved (live) version
            new ProductVersion
            {
                Id = new Guid("aaaaaaaa-0000-0000-0000-000000000001"),
                ProductId = productA,
                VersionNumber = 1,
                Name = "Wireless Mouse",
                Description = "2.4GHz optical wireless mouse.",
                Price = 299.99m,
                Sku = "MSE-WL-001",
                Status = WorkflowStatus.Approved,
                CreatedBy = capturer,
                CreatedAt = new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc),
                DecidedBy = manager,
                DecidedAt = new DateTime(2026, 5, 2, 10, 0, 0, DateTimeKind.Utc)
            },

            // Product B — v1 approved (live) AND v2 pending (edit awaiting approval)
            new ProductVersion
            {
                Id = new Guid("bbbbbbbb-0000-0000-0000-000000000001"),
                ProductId = productB,
                VersionNumber = 1,
                Name = "Mechanical Keyboard",
                Description = "Tactile mechanical keyboard, blue switches.",
                Price = 899.99m,
                Sku = "KBD-MEC-001",
                Status = WorkflowStatus.Approved,
                CreatedBy = capturer,
                CreatedAt = new DateTime(2026, 5, 3, 8, 0, 0, DateTimeKind.Utc),
                DecidedBy = manager,
                DecidedAt = new DateTime(2026, 5, 4, 9, 0, 0, DateTimeKind.Utc)
            },
            new ProductVersion
            {
                Id = new Guid("bbbbbbbb-0000-0000-0000-000000000002"),
                ProductId = productB,
                VersionNumber = 2,
                Name = "Mechanical Keyboard (RGB)",
                Description = "Adds RGB backlighting and a volume wheel.",
                Price = 999.99m,
                Sku = "KBD-MEC-001",
                Status = WorkflowStatus.Pending,
                CreatedBy = capturer,
                CreatedAt = new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc)
            },

            // Product C — v1 rejected, v2 draft (reworking after rejection)
            new ProductVersion
            {
                Id = new Guid("cccccccc-0000-0000-0000-000000000001"),
                ProductId = productC,
                VersionNumber = 1,
                Name = "USB-C Hub 7-in-1",
                Description = "7-port USB-C hub with HDMI and PD.",
                Price = 549.99m,
                Sku = "HUB-USBC-7",
                Status = WorkflowStatus.Rejected,
                CreatedBy = capturer,
                CreatedAt = new DateTime(2026, 5, 5, 8, 0, 0, DateTimeKind.Utc),
                DecidedBy = manager,
                DecidedAt = new DateTime(2026, 5, 6, 11, 0, 0, DateTimeKind.Utc),
                DecisionReason = "SKU already in use on another product; correct and resubmit."
            },
            new ProductVersion
            {
                Id = new Guid("cccccccc-0000-0000-0000-000000000002"),
                ProductId = productC,
                VersionNumber = 2,
                Name = "USB-C Hub 7-in-1",
                Description = "7-port USB-C hub with HDMI and PD.",
                Price = 549.99m,
                Sku = "HUB-USBC-701",
                Status = WorkflowStatus.Draft,
                CreatedBy = capturer,
                CreatedAt = new DateTime(2026, 6, 10, 8, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<ProductReadModel>().HasData(
            new ProductReadModel
            {
                ProductId = new Guid("11111111-1111-1111-1111-111111111111"),
                Name = "Wireless Mouse",
                Description = "2.4GHz optical wireless mouse.",
                Price = 299.99m,
                Sku = "MSE-WL-001",
                VersionNumber = 1,
                VersionId = new Guid("aaaaaaaa-0000-0000-0000-000000000001"),
                ApprovedBy = "manager@moyo.com",
                ApprovedAt = new DateTime(2026, 5, 2, 10, 0, 0, DateTimeKind.Utc)
            },
            new ProductReadModel
            {
                ProductId = new Guid("22222222-2222-2222-2222-222222222222"),
                Name = "Mechanical Keyboard",
                Description = "Tactile mechanical keyboard, blue switches.",
                Price = 899.99m,
                Sku = "KBD-MEC-001",
                VersionNumber = 1,
                VersionId = new Guid("bbbbbbbb-0000-0000-0000-000000000001"),
                ApprovedBy = "manager@moyo.com",
                ApprovedAt = new DateTime(2026, 5, 4, 9, 0, 0, DateTimeKind.Utc)
            }
        );

        //Add to end of migration
        //migrationBuilder.Sql("UPDATE Products SET CurrentApprovedVersionId = 'aaaaaaaa-0000-0000-0000-000000000001' WHERE Id = '11111111-1111-1111-1111-111111111111';");
        //migrationBuilder.Sql("UPDATE Products SET CurrentApprovedVersionId = 'bbbbbbbb-0000-0000-0000-000000000001' WHERE Id = '22222222-2222-2222-2222-222222222222';");
    }

}