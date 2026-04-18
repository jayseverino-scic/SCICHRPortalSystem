using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserAccountOnUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "UserId", "ContactNumber", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsApproved", "IsPasswordChanged", "LastName", "Locked", "LoginAttempts", "MiddleName", "Password", "Salt", "UpdatedAt", "UpdatedBy", "Username" },
                values: new object[] { 1, null, new DateTime(2025, 4, 18, 10, 30, 0, 0, DateTimeKind.Utc), "jun rivas", "superadmin@mail.com", "Super", true, false, "Admin", false, 0, null, "4DRtkqzRrxUk9Px/+Zu7vzTIk5f0dHc4mPgicSMkQzI=", "ml4A7caIeJit28zFyeiXVA==", null, null, "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 1);
        }
    }
}
