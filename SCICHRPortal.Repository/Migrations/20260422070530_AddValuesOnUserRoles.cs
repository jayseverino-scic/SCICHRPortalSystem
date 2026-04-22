using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddValuesOnUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "RoleId", "CreatedAt", "CreatedBy", "Description", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 4, 18, 10, 30, 0, 0, DateTimeKind.Utc), "jun rivas", "Administrator", "Administrator", null, null },
                    { 2, new DateTime(2025, 4, 18, 10, 30, 0, 0, DateTimeKind.Utc), "jun rivas", "Other", "Other", null, null }
                });

            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "UserRoleId", "CreatedAt", "CreatedBy", "RoleId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { 1, new DateTime(2025, 4, 18, 10, 30, 0, 0, DateTimeKind.Utc), "jun rivas", 1, null, null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "RoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserRole",
                keyColumn: "UserRoleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "RoleId",
                keyValue: 1);
        }
    }
}
