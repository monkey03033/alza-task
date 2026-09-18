using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alza.Delivery.DataMigrations.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    VanTripId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_packages_unassigned",
                table: "Packages",
                column: "VanTripId",
                filter: "\"VanTripId\" IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "Id", "Weight", "Volume", "Revenue" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Packages");
        }
    }
}
