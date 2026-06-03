using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AE.Market.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateUserPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "auth",
                table: "user_permissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "auth",
                table: "user_permissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "auth",
                table: "user_permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                schema: "auth",
                table: "user_permissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "auth",
                table: "user_permissions");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "auth",
                table: "user_permissions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "auth",
                table: "user_permissions");

            migrationBuilder.DropColumn(
                name: "LastModified",
                schema: "auth",
                table: "user_permissions");
        }
    }
}
