using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SystemChallengeAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "CurrentApprovedVersionId", "DeletedAt", "DeletedBy", "IsDeleted" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Utc), "capturer@moyo.com", null, null, null, false },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 5, 3, 8, 0, 0, 0, DateTimeKind.Utc), "capturer@moyo.com", null, null, null, false },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 5, 5, 8, 0, 0, 0, DateTimeKind.Utc), "capturer@moyo.com", null, null, null, false }
                });

            migrationBuilder.InsertData(
                table: "ProductVersions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DecidedAt", "DecidedBy", "DecisionReason", "Description", "Name", "Price", "ProductId", "Sku", "Status", "VersionNumber" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000001"), new DateTime(2026, 5, 1, 8, 0, 0, 0, DateTimeKind.Utc), "capturer@moyo.com", new DateTime(2026, 5, 2, 10, 0, 0, 0, DateTimeKind.Utc), "manager@moyo.com", null, "2.4GHz optical wireless mouse.", "Wireless Mouse", 299.99m, new Guid("11111111-1111-1111-1111-111111111111"), "MSE-WL-001", "Approved", 1 },
                    { new Guid("bbbbbbbb-0000-0000-0000-000000000001"), new DateTime(2026, 5, 3, 8, 0, 0, 0, DateTimeKind.Utc), "capturer@moyo.com", new DateTime(2026, 5, 4, 9, 0, 0, 0, DateTimeKind.Utc), "manager@moyo.com", null, "Tactile mechanical keyboard, blue switches.", "Mechanical Keyboard", 899.99m, new Guid("22222222-2222-2222-2222-222222222222"), "KBD-MEC-001", "Approved", 1 },
                    { new Guid("bbbbbbbb-0000-0000-0000-000000000002"), new DateTime(2026, 6, 1, 8, 0, 0, 0, DateTimeKind.Utc), "capturer@moyo.com", null, null, null, "Adds RGB backlighting and a volume wheel.", "Mechanical Keyboard (RGB)", 999.99m, new Guid("22222222-2222-2222-2222-222222222222"), "KBD-MEC-001", "Pending", 2 },
                    { new Guid("cccccccc-0000-0000-0000-000000000001"), new DateTime(2026, 5, 5, 8, 0, 0, 0, DateTimeKind.Utc), "capturer@moyo.com", new DateTime(2026, 5, 6, 11, 0, 0, 0, DateTimeKind.Utc), "manager@moyo.com", "SKU already in use on another product; correct and resubmit.", "7-port USB-C hub with HDMI and PD.", "USB-C Hub 7-in-1", 549.99m, new Guid("33333333-3333-3333-3333-333333333333"), "HUB-USBC-7", "Rejected", 1 },
                    { new Guid("cccccccc-0000-0000-0000-000000000002"), new DateTime(2026, 6, 10, 8, 0, 0, 0, DateTimeKind.Utc), "capturer@moyo.com", null, null, null, "7-port USB-C hub with HDMI and PD.", "USB-C Hub 7-in-1", 549.99m, new Guid("33333333-3333-3333-3333-333333333333"), "HUB-USBC-701", "Draft", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductVersions",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ProductVersions",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ProductVersions",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "ProductVersions",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ProductVersions",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));
        }
    }
}
