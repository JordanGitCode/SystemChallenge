using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SystemChallengeAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ProductReadModel",
                columns: new[] { "ProductId", "ApprovedAt", "ApprovedBy", "Description", "Name", "Price", "Sku", "VersionId", "VersionNumber" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 5, 2, 10, 0, 0, 0, DateTimeKind.Utc), "manager@moyo.com", "2.4GHz optical wireless mouse.", "Wireless Mouse", 299.99m, "MSE-WL-001", new Guid("aaaaaaaa-0000-0000-0000-000000000001"), 1 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 5, 4, 9, 0, 0, 0, DateTimeKind.Utc), "manager@moyo.com", "Tactile mechanical keyboard, blue switches.", "Mechanical Keyboard", 899.99m, "KBD-MEC-001", new Guid("bbbbbbbb-0000-0000-0000-000000000001"), 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductReadModel",
                keyColumn: "ProductId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "ProductReadModel",
                keyColumn: "ProductId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));
        }
    }
}
