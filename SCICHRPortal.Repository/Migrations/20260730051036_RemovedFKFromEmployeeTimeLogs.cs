using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCICHRPortal.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemovedFKFromEmployeeTimeLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "EmployeeTimeLog",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTimeLog_EmployeeId",
                table: "EmployeeTimeLog",
                newName: "IX_EmployeeTimeLog_id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_id",
                table: "EmployeeTimeLog",
                column: "id",
                principalTable: "XEmployee",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_id",
                table: "EmployeeTimeLog");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "EmployeeTimeLog",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTimeLog_id",
                table: "EmployeeTimeLog",
                newName: "IX_EmployeeTimeLog_EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeLog_XEmployee_EmployeeId",
                table: "EmployeeTimeLog",
                column: "EmployeeId",
                principalTable: "XEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
