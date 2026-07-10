using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemChallengeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Sequence",
                table: "ProductReadModel",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReadModel_Sequence",
                table: "ProductReadModel",
                column: "Sequence",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductReadModel_Sequence",
                table: "ProductReadModel");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "ProductReadModel");
        }
    }
}
