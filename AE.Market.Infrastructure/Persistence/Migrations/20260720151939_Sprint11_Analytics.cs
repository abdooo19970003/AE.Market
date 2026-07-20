using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AE.Market.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint11_Analytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                schema: "catalog",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "search_analytics",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Filters = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResultCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LatencyMs = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    SearchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_analytics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_search_analytics_SearchedAt",
                schema: "analytics",
                table: "search_analytics",
                column: "SearchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_search_analytics_SearchText",
                schema: "analytics",
                table: "search_analytics",
                column: "SearchText");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "search_analytics",
                schema: "analytics");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                schema: "catalog",
                table: "products");
        }
    }
}
